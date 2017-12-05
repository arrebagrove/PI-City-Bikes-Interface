PI City Bikes Interface
===

## Introduction
Custom interface developed using PI AF SDK which retrieves data from CityBikes API and stores it on the PI System.

## Installation

Copy all the files from the dist folder to the **%PIHOME%\Interface\PICityBikes**. 

Then run the following commands to create the Windows Service:

```
PICityBikes.Service.exe --install
```
Delete the Windows Service using the command below:

```
PICityBikes.Service.exe --uninstall
```

## Configuration

Before starting the interface, open **PICityBikes.Service.exe.config** and edit it according to the information below:


Parameter | Description
------------ | -------------
piServer | PI Data Archive name
afServer | AF Server name
afDatabase | AF Database name
secondsToWait | Number of seconds idle between cycles. Each cycle gets the full data available on the API.
createObjects | If it is true, the interface will create AF element and attribute templates as well as missing PI Points. If set to false, it will get the values directly assuming that the required objects already exist on the system.

## Troubleshooting

The interface will save the logs in the **PICityBikes.Core.log** file located on the interface folder.


## Licensing
Copyright 2017 OSIsoft, LLC.

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
   
Please see the file named [LICENSE.md](LICENSE.md).
