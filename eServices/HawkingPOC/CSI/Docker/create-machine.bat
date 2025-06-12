@set /p name=Machine Name: 
@set /p pwd=Password for %USERNAME%: 

docker-machine rm %name%

docker-machine create -d hyperv --hyperv-virtual-switch "External" --hyperv-cpu-count 4 --hyperv-memory "4096" --engine-env HTTP_PROXY=http://%USERNAME%:%pwd%@proxy.wtg.zone:8080 --engine-env HTTPS_PROXY=http://%USERNAME%:%pwd%@proxy.wtg.zone:8080 --engine-env NO_PROXY=.wtg.zone %name%

for /f "skip=1 tokens=1,5" %%a IN ('docker-machine ls') DO if %%a==%name% (set machine_url=%%b)
for /f "delims=/: tokens=2" %%a in ("%machine_url%") do set machine_ip=%%a

docker-machine ssh %name% "echo $'export HTTP_PROXY=http://%USERNAME%:%pwd%@proxy.wtg.zone:8080' | sudo tee -a /etc/profile"
docker-machine ssh %name% "echo $'export HTTPS_PROXY=http://%USERNAME%:%pwd%@proxy.wtg.zone:8080' | sudo tee -a /etc/profile"
docker-machine ssh %name% "echo $'export NO_PROXY=.wtg.zone' | sudo tee -a /etc/profile"

"D:\Tools\putty\PSCP.EXE" "D:\HawkingSoftware\CA-Certs\wtg.crt" docker@%machine_ip%:
docker-machine ssh %name% "sudo mkdir /var/lib/boot2docker/certs"
docker-machine ssh %name% "sudo cp wtg.crt /var/lib/boot2docker/certs/"

docker-machine ssh %name% "echo $'EXTRA_ARGS=\"--insecure-registry sydco-wbln-2.wtg.zone:5000\"' | sudo tee -a /var/lib/boot2docker/profile && sudo /etc/init.d/docker restart"

docker-machine restart %name%
docker-machine env %name%
