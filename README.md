# TimeTriggerEmail

Azure Function linked to TravelBot.
This feature periodically checks (Friday at 7pm) the price of flights saved by users in order to send a notification/email if there is a price change.

| Service/Resource | Description |
| ---------- | -------- | 
| SERVER SQL | SQL Server installed and hosted in the cloud runs in Windows Server virtual machines |
| DATABASE SQL | A simple DB to store all the City and Airport known, and to save client's demand  |
| STORAGE ACCOUNT - CONTAINER - BLOB | Storing files for distributed access, (images,html, json)  |
| AZURE COMUNICATION SERVICE | Which will allow you to send emails |
| AZURE EMAIL COMUNICATION SERVICE | Which will allow to add Domain Name |
| AZURE FUNCTION| Used to run time trigger logic |

![image](https://github.com/UniRoby/TravelBot/assets/107865801/6e48eba4-c708-4437-b86d-50c1147b7b27)


All the Service and Resources are in the same Resources Group

## Setup
- An active Azure account and student subscription. 
- Visual Studio 2022
-  .NET Core. 7.0
- Time Trigger Function

## Deployment Steps

 ## Create Visual Studio Project
     - Set the CRON to send emails at 7pm every Friday : 00 19 * * 5
##Publish Solution
On Visual Studio:
-Right Click on the solution name
-Publish
-Select Azure
-Select Azure Function App (Windows)
-Create New Function App: (TimeTriggerEmail20240210221352), Fill all the input of the form
-Select Finish and click on Pulish button
-Insert all the secrets in the configuration settings


###DEMO
![demoEmail2](https://github.com/UniRoby/TravelBot/assets/107865801/97ccb878-9f54-40f0-b6bc-f282bfa13056)

![demoEmail](https://github.com/UniRoby/TravelBot/assets/107865801/80934ea0-7277-4b3a-a754-30ed74160999)
