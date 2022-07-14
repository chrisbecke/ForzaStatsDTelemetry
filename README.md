# ForzaStatsDTelemetry
A StatsD adapter for Forza  Horizon 4 telemetry.
This may or may not work with other versions of Forza Horizon or Motorsport but the telemetry data layout changes and I've only tested with Horizon 4.

## Getting Started

You will need Docker Desktop, or a server to run Docker on.  
Visual Studio, to build this app.  
Forza Horizon 4 for the XBOX or PC.  

## Enable localhost loopback for Forza on Windows 10

Windows Store applications are blocked from connecting to localhost. If the statsd listner and Horizon are both on the same Desktop, then an excemption can be created. 

```
CheckNetIsolation.exe LoopbackExempt -a -n=microsoft.apollobasegame_8wekyb3d8bbwe
```

## Deploying with Docker-Compose

### Deploy with docker-compose
```bash
docker-compose up
```

### Deploy to Docker Swarm

```bash
docker stack deploy -c docker-compose.yml forza-listner
```

### The compose file

```yaml
version: "3.7"

services:
  forzalistner:
    build: ForzaListner/Dockerfile
    context: .
    ports:
      - 5200:5200

  grafana:
    image: grafana/grafana
    ports:
      - 3000:3000
    environment:
      GF_INSTALL_PLUGINS: "grafana-clock-panel,grafana-simple-json-datasource"
      GF_SECURITY_ADMIN_PASSWORD: "secret"
    volumes:
      - data:/var/lib/grafana
      
  graphite:
    image: graphiteapp/graphite-statsd
    ports:
      - "3001:80"
      - 2003-2004:2003-2004
      - 2023-2024:2023-2024
      - 8125:8125/udp
      - 8126:8126
    environment:
#      MEMCACHE_HOST: "memcached:11211"
      CACHE_DURATION: 60
    volumes:
      - logs:/var/log
      - logrotate_etc:/etc/logrotate.d
      - nginx_etc:/etc/nginx
      - conf:/opt/graphite/conf
      - storage:/opt/graphite/storage
      - custom:/opt/graphite/webapp/graphite/functions/custom
      - statsd_etc:/opt/statsd/config
      - redis:/var/lib/redis    
```

## Hooking it up

The Settings menu in Forza Horizon has a menu option called "Data Out" Set the host to be "localhost" - or wherever you are running the collector and the port to be "5200".
