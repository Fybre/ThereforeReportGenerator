using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Dynamic;
using ThereforeReportGenerator.Models;

namespace ThereforeReportGenerator.Controllers
{
    class WorkflowDetailProcessor
    {
        const int MAX_ROWS = int.MaxValue;
        const int WORKFLOW_FLAG_ALL_RUNNING = 1;
        const int USER_SINGLE = 1;
        const int USER_GROUP = 2;
        const int USER_SYSTEM = 3;
        private TimeSpan _processTime;
        private ReportConfiguration _rc;
        public TimeSpan processTime
        {
            get { return _processTime; }
        }
        public string lastError { get; set; } = "";
        public event EventHandler<ProgressEventArgs>? OnProgress;

        public WorkflowDetailProcessor(ReportConfiguration rc)
        {
            _rc=rc;
        }

        /// <summary>
        /// Retrieves all workflow instances for all workflows, or a subset of workflows.
        /// </summary>
        /// <param name="maxRows">Maximum no of rows to return from the query</param>
        /// <param name="wfProcesses">The wf process numbers to return results for, leave as null to process all workflow processes</param>
        /// <returns></returns>
        public async Task<List<InstanceDetail>> GetAllInstancesAsync(int maxRows = MAX_ROWS, List<int>? wfProcesses = null)
        {
            var res = new List<InstanceDetail>();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                List<int> instances = new List<int>();
                if (wfProcesses == null)
                {
                    instances.AddRange(await ExecutWorkflowQueryForAllOrForProcessAsync(maxRows));
                }
                else
                {
                    foreach (var wf in wfProcesses)
                    {
                        instances.AddRange(await ExecutWorkflowQueryForAllOrForProcessAsync(maxRows, wf));
                    };
                }

                var count = 0;
                foreach (var instance in instances)
                {
                    count++;
                    List<InstanceDetail> details = await GetDetailForSingleInstanceAsync(instance);
                    
                    //trigger event publish
                    OnProgress?.Invoke(this, new ProgressEventArgs
                    {
                        CurrentProcess = ProgressEventArgs.ProcessingType.Querying,
                        CurrentInstanceCount = count,
                        TotalInstanceCount = instances.Count,
                        InstanceNo = instance,
                        ProcessName = details.FirstOrDefault(x=> !string.IsNullOrEmpty(x.ProcessName))?.ProcessName,
                        TaskName = details.FirstOrDefault(x => !string.IsNullOrEmpty(x.TaskName))?.TaskName, 
                        IndexDataString = details.FirstOrDefault(x => !string.IsNullOrEmpty(x.IndexDataString))?.IndexDataString??""
                    });
                    res.AddRange(details);
                }
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
            }
            stopwatch.Stop();
            _processTime = stopwatch.Elapsed;
            return res;
        }

        private async Task<List<InstanceDetail>> GetDetailForSingleInstanceAsync(int instance)
        {
            var instanceDetails = await GetWorkflowInstanceAsync(instance);
            List<UserDetail> fullWfUserList = new List<UserDetail>();
            await AddUsersAsync(fullWfUserList, instanceDetails.AssignedToUsers);
            instanceDetails.AssignedToUsers = fullWfUserList;
            return instanceDetails.GetFlattenedUserList();
        }

        private async Task AddUsersAsync(List<UserDetail> finalUserList, List<UserDetail> assignedUsers)
        {
            foreach (var i in assignedUsers)
            {
                if (i.UserType == USER_GROUP)
                {
                    var groupUsers = await GetUsersFromGroupAsync((int)i.UserId);
                    await AddUsersAsync(finalUserList, groupUsers);
                }
                else if (i.UserType == USER_SINGLE && !(bool)i.Disabled)
                {
                    finalUserList.Add(i);
                }
            }
        }

        private async Task<InstanceDetail> GetWorkflowInstanceAsync(int instanceNo)
        {
            string endPointUrl = $"{_rc.TenantBaseUrl}/GetWorkflowInstance";

            dynamic GetWorkflowInstanceData = new ExpandoObject();
            GetWorkflowInstanceData.InstanceNo = instanceNo;
            GetWorkflowInstanceData.TokenNo = 0;
            var jsonContent = await SendRequestAsync(HttpMethod.Post, endPointUrl, _rc.TenantHeaders, GetWorkflowInstanceData);
            if (jsonContent == null) { return new InstanceDetail(); }
            var jObject = JObject.Parse(jsonContent);
            var wfInstance = jObject["WorkflowInstance"];

            var indexString = string.Empty;

            ((JArray)jObject["LinkedDocuments"] ?? []).ToList().ForEach(x => indexString += (string?)x["IndexDataString"] ?? "");
            var res = new InstanceDetail
            {
                ProcessName = (string?)wfInstance["ProcessName"],
                InstanceNo = (int)wfInstance?["InstanceNo"],
                ProcessNo = (int)wfInstance["ProcessNo"],
                TaskName = (string?)wfInstance["CurrTaskName"],
                TaskStart = (DateTime)wfInstance["TaskStartDate"],
                ProcessStartDate = (DateTime)wfInstance["ProcessStartDate"],
                TaskDue = ((DateTime)wfInstance["TaskDueDate"]).Year > 2000 ? (DateTime)wfInstance["TaskDueDate"] : null,
                IndexDataString = indexString,
                TWAUrl = _rc.TenantUrl
            };
            JArray assignedToUsers = (JArray?)jObject["WorkflowInstance"]?["AssignedToUsers"] ?? [];
            foreach (var userId in assignedToUsers)
            {
                var userDetail = await GetUserDetailAsync((int)userId);
                res.AssignedToUsers.Add(userDetail);
            }
            return res;
        }

        private async Task<List<UserDetail>> GetUsersFromGroupAsync(int groupID)
        {
            List<UserDetail> res = new List<UserDetail>();
            string endPointUrl = $"{_rc.TenantBaseUrl}/GetUsersFromGroup";
            dynamic GetUsersFromGroupData = new ExpandoObject();
            GetUsersFromGroupData.GroupId = groupID;
            var jsonContent = await SendRequestAsync(HttpMethod.Post, endPointUrl, _rc.TenantHeaders, GetUsersFromGroupData);
            if (jsonContent == null) { return res; }
            var jObject = JObject.Parse(jsonContent);
            JArray users = (JArray?)jObject["Users"] ?? [];
            foreach (var user in users)
            {
                if (!(bool)user["Disabled"])
                {
                    res.Add(new UserDetail
                    {
                        DisplayName = (string?)user["DisplayName"],
                        Smtp = (string?)user["SMTP"],
                        UserType = (int)user["UserType"]
                    });
                }
            }
            return res;
        }

        private async Task<UserDetail?> GetUserDetailAsync(int userId)
        {
            string endPointUrl = $"{_rc.TenantBaseUrl}/GetUserDetails";
            dynamic GetUserDetaildata = new ExpandoObject();
            GetUserDetaildata.UserOrGroupId = userId;
            var jsonContent = await SendRequestAsync(HttpMethod.Post, endPointUrl, _rc.TenantHeaders, GetUserDetaildata);
            if (jsonContent == null) { return null; }
            var jObject = JObject.Parse(jsonContent);

            var res = new UserDetail
            {
                DisplayName = (string?)jObject?["UserDetails"]?["DisplayName"],
                Smtp = (string?)jObject?["UserDetails"]?["SMTP"],
                UserType = (int?)jObject?["UserDetails"]?["UserType"],
                Disabled = ((string)jObject?["Disabled"]) == "true" ? true : false,
                UserId = (int?)jObject?["UserDetails"]?["UserId"]
            };
            return res;
        }

        private async Task<List<int>> ExecutWorkflowQueryForAllOrForProcessAsync(int maxRows, int? processNo = null)
        {
            var res = new List<int>();
            string endPointUrl = (processNo == null) ? $"{_rc.TenantBaseUrl}/ExecuteWorkflowQueryForAll" : $"{_rc.TenantBaseUrl}/ExecuteWorkflowQueryForProcess";
            dynamic ExecuteWorflowQueryForAllData = new ExpandoObject();
            ExecuteWorflowQueryForAllData.WorkflowFlags = WORKFLOW_FLAG_ALL_RUNNING;
            ExecuteWorflowQueryForAllData.MaxRows = maxRows;
            if (processNo != null) { ExecuteWorflowQueryForAllData.ProcessNo = processNo; }

            var jsonContent = await SendRequestAsync(HttpMethod.Post, endPointUrl, _rc.TenantHeaders, ExecuteWorflowQueryForAllData);
            if (jsonContent == null || string.IsNullOrEmpty(jsonContent)) { return res; }

            var jObject = JObject.Parse(jsonContent);
            if (processNo == null)
            {
                ((JArray?)jObject["WorkflowQueryResultList"])?.ToList().ForEach(x =>
                        ((JArray)x["ResultRows"]).ToList().ForEach(y => res.Add((int)y["InstanceNo"])));
            }
            else
            {
                ((JArray)jObject["WorkflowQueryResult"]["ResultRows"]).ToList().ForEach(x => res.Add((int)x["InstanceNo"]));
            }
            return res;
        }

        private async Task<string?> SendRequestAsync(HttpMethod method, string url, Dictionary<string, string> headers, object data)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(method, url);
            foreach (var keyValue in headers)
            {
                request.Headers.Add(keyValue.Key, keyValue.Value);
            }
            var contentData = JsonConvert.SerializeObject(data);
            var content = new StringContent(contentData, null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : null;
        }
    }

}
