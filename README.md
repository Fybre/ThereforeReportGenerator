# Therefore Report Generator
Create bulk workflow reports for users, rather than recieving one email notification per worklow. Scheduled via cron.

### Sample Docker Compose entry
```yaml
  thereforereportgenerator:
    image: fybre/thereforereportgenerator:latest
    container_name: thereforereportgenerator
    restart: 'unless-stopped'
    environment:
      - TZ=Australia/Sydney
      - LC_ALL=en_AU.UTF-8
    volumes:
      - /home/craig/docker/thereforereportgenerator/Data:/app/Data
    networks:
      - shared-network
```

Set the environment variables to enable correct localisation. Locales supported are `en_AU.UTF-8` and `en_US.UTF-8` for date formatting.

Pull via
`docker pull fybre/thereforeworkflowrunner`

Suggested to run this behind a cloudflare tunnel.
