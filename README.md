# MySQL vs CockroachDB Benchmark

## Configuration

The benchmark requires a MySQL and a CockroachDB server.  Copy the file `config.json.example` to `config.json`.  Then edit the `config.json` file with the appropriate connection strings for each server.

## Concurrency Testing

To run concurrency tests, execute the following command

```
dotnet run concurrency [iterations] [concurrency] [operations]
```
