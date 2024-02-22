# Repository for Travel Bot
![alt text](https://storageaccountprojcloud.blob.core.windows.net/container-progetto-cloud/EmailLogo.png?sp=r&st=2024-02-02T18:26:22Z&se=2024-08-31T22:00:00Z&spr=https&sv=2022-11-02&sr=c&sig=xv6vvVpt901525Ld6IWdEFz7dXaR%2Fz%2BaHA%2F3kdhoI6A%3D)

This is a project for The Cloud Computing Exam usign the Azure's services.

| Service/Resource | Description |
| ---------- | -------- | 
| Azure Cognitive Service CLU |  Conversational language understanding service to ask the bot to search flight  |
| SERVER SQL | SQL Server installed and hosted in the cloud runs in Windows Server virtual machines |
| DATABASE SQL | A simple DB to store all the City and Airport known, and to save client's demand  |
| STORAGE ACCOUNT - CONTAINER - BLOB | Storing files for distributed access, (images,html, json)  |
| BOT | Which will allow you to register your bot with the Azure AI Bot Service |

# TRAVEL BOT 
All the Service and Resources are in the same Resources Group

## Setup
- An active Azure account and student subscription. 
- Visual Studio 2022
-  .NET Core. 7.0
- Bot Framework SDK

## Deployment Steps

### 1. Create the Azure Bot Service Resource

Begin by creating an Azure Bot Service resource.
User-assigned managed identity was selected.
Create new Microsoft App ID was selected.

### 2. Obtain Bot's App ID and Password

After creating the Azure Bot Service resource, retrieve the Microsoft app ID and password assigned to your bot upon deployment. 

### 3. Create a Web App to Host Bot Logic

To host your bot's logic, you can either customize Bot Builder samples to fit your scenario or utilize the Bot Builder SDK to create a web app. 

- Azure Bot Service expects the Bot Application Web App Controller to expose an endpoint in the form `/api/messages` to handle all messages sent to the bot.

### Create App Service Plan via Azure Portal:
- In the Azure Portal, click on “Create a resource.”
- Search for “App Service Plan” and select it.
- Click “Create” to start the setup process.
- Fill in the required details, such as the name, subscription, resource group, and operating system for the plan.


#### Creating the Bot Web App via Azure Portal:

- Navigate to the Azure portal.
- Select "Create a resource" and search for "web app."
- Choose the Web App tile to create the app.

-In the portal, select Create a resource. In the search box, enter web app. Select the Web App tile: TravelBotAppCloud

### Details for the app
-Publish: Code
-RunTime stack: .NET Core 7.0 LTS
-Operating system: Windows
-Region: West Europe

#Deploy TravelBot Code to TravelBotAppCloud
1 - In Visual Studio, in Solution Explorer, right-click the TravelBot project and select Publish
2 - Select Azure 
3 - Select App Service
4 - In the deployment configuration, select the web app TravelBotAppCloud. 
5 - Select Finish, and then select Publish to start the deployment

Alle the secrets used in TravelBot are stored in the application settings.

###Test The Bot
- Go to the Azure Bot resource  and click on the Test in Web Chat from the left panel

###DEMO

![demo2_1](https://github.com/UniRoby/TravelBot/assets/107865801/f3d68b30-ea49-455d-9dfc-b5d57dac9556)

![demo2](https://github.com/UniRoby/TravelBot/assets/107865801/7c3729d0-87dd-4a93-8da7-b2ee840d27fe)

![demo3](https://github.com/UniRoby/TravelBot/assets/107865801/7f36d409-0692-4a15-ae26-8bd943a29512)

![realFlights](https://github.com/UniRoby/TravelBot/assets/107865801/5a77dd99-ebab-47ab-9d2d-89f8ddf7bdbf)


  
