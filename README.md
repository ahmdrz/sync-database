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
      "name": "<table>",
      "column" : "<checkConflictColumns>"
    }
  ]
}
```

Replace your variables and save it to `config.json`


### Attention 

May have some problem with `nvarchar` data type.

### ToDo

1. Put it to a class
2. Fix `nvarchar` problem , using `Parameterized Query` [read](https://msdn.microsoft.com/library/bb738521(v=vs.100).aspx)
