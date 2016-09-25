# sync-database
Very Very Simple Program to sync two databases.

It's work very simply. 

I wrote a program to sync two databases.

**Note** This program may confilict records. The primary keys and unique keys should be differents.

I upload this code just for keep it on clouds. If you want to use it , First of all read the code and make sure that your databases haven't confilicts.

### Dependencies

> Open the console. " View" > "Other Windows" > "Package Manager Console"

> Then type the following: Install-Package Newtonsoft.Json.

### Configuration

```json
{
  "server": "<connectionstring>",
  "local": "<connectionstring>",
  "servertolocal": true,
  "tables": [
    {
      "name": "<table>"
    }
  ]
}
```

Replace your variables and save it to `config.json`
