# Building the Basics Station Module

Open a command prompt at this directory and make sure you have Docker (Desktop) installed.

```
docker build -f .\Dockerfile.<cpu arch> .
```

# Running the Basics Station Module

```
docker run -it -e TC_URI=ws://<ipaddress:port of lora network server> <docker image id of previous build step>
```
