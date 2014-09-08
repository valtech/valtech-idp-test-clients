set -e

while true ; do netcat -l -p 55066 -e ./server.sh; test $? -gt 128 && break ; done
