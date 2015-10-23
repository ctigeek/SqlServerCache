##Remove the contents of packages.config so it doesn't create dependencies in this package.
nuget pack .\SqlServerCacheClient.csproj -Prop Configuration=Release