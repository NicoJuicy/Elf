# Elf

![Azure Web App Build and Deploy](https://github.com/EdiWang/Elf/workflows/Azure%20Web%20App%20Build%20and%20Deploy/badge.svg) ![Docker Build and Push](https://github.com/EdiWang/Elf/workflows/Docker%20on%20Linux/badge.svg) ![ACR Build and Push](https://github.com/EdiWang/Elf/workflows/ACR%20Build%20and%20Push/badge.svg)

The link forward service used by https://go.edi.wang. It generates static URLs for redirecting third party URLs. It's similar to, but **NOT a URL shorter**. 

- Use a static token to adapt changes to origin url.
- Track user click to generate report. (Only if DNT isn't enabled)

e.g.:

Raw URL:
```
https://www.somewebsite.com/a-very-long-and-complicated-link-that-can-also-change?with=parameters
```

will be translate to `https://yourdomain/fw/token` or `https://yourdomain/aka/name`

## Features

Forward Link, Create/Manage/Share Link, View Report.

![image](https://user-images.githubusercontent.com/3304703/156873651-845c72fe-84e7-4536-bfe4-6785accae5ce.png)
![image](https://user-images.githubusercontent.com/3304703/156873643-38f9d582-777e-4cb1-b053-6656fb5bd36e.png)

## Forward Logic

![image](https://ediwang.cdn.moonglade.blog/web-assets/lf/LinkForwarder-FW.png)

## Docker Deployment

https://hub.docker.com/r/ediwang/elf

You can also follow the next section to build and run the project yourself.

## Build and Run API

Tools | Alternative
--- | ---
[.NET 6 SDK](http://dot.net) | N/A
[Visual Studio 2022](https://visualstudio.microsoft.com/) | [Visual Studio Code](https://code.visualstudio.com/)
[Azure SQL Database](https://azure.microsoft.com/en-us/services/sql-database/) | [SQL Server 2019](https://www.microsoft.com/en-us/sql-server/sql-server-2019) / LocalDB (Dev Only)

For a quick Azure deployment, you can use the automation script ```Azure-Deployment\Deploy.ps1``` to setup a ready-to-run Elf in a couple of minutes. (Azure CLI is required to run the script)

### Setup Database

[Create an Azure SQL Database](https://docs.microsoft.com/en-us/azure/sql-database/sql-database-single-database-get-started?WT.mc_id=AZ-MVP-5002809) or a SQL Server 2019+ database. e.g. elf

Update the connection string "**ElfDatabase**" in **appsettings.[env].json**

```json
"ConnectionStrings": {
  "ElfDatabase": "Server=(localdb)\\MSSQLLocalDB;Database=elf;Trusted_Connection=True;"
}
```
### Build Source

Build and run `./src/API/Elf.sln`

### Authentication

Register an App in **[Azure Active Directory]((https://azure.microsoft.com/en-us/services/active-directory/))**
- Set an **Application ID URI** as unique for this app
- Expose an API with name `access_as_user` and with **Admins and users** type
- Change `accessTokenAcceptedVersion` to `2` in Manifest blade
- Copy "**appId**" to set as **AzureAd:ClientId** in **appsettings.json** file

```json
"AzureAd": {
  "Domain": "{YOUR-VALUE}",
  "TenantId": "{YOUR-VALUE}",
  "ClientId": "{YOUR-VALUE}",
}
```

### Azure Cache for Redis (Optional)

To use Redis, follow these steps:

1. Create an [Azure Cache for Redis instance](https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-overview?WT.mc_id=AZ-MVP-5002809)
2. Copy the connection string in "Access keys"
3. Set the connection string in `ConnectionStrings:RedisConnection` in `appsettings.json` or environment variable
4. Set `AppSettings:UseRedis` to `true` in `appsettings.json` or environment variable
5. Restart the application
