using DatabaseComparer.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace DatabaseComparer.Business
{
    public class CompareManager
    {
        private readonly ILogger<CompareManager> _logger;
        private readonly IConfiguration _configuration;

        public CompareManager(ILogger<CompareManager> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public List<CompareResult> GetCompares()
        {
            ConnectionDetail[] con_secil = _configuration.GetSection("ConnectionDetail").Get<ConnectionDetail[]>();
            try
            {
                Dictionary<string, List<Routine>> routines = new Dictionary<string, List<Routine>>();
                foreach (var item in con_secil)
                {
                    NpgsqlConnection con = new NpgsqlConnection(item.Connection);
                    con.Open();
                    NpgsqlCommand cmd_secil = new NpgsqlCommand("SELECT get()", con);
                    string json = cmd_secil.ExecuteScalar().ToString();
                    List<Routine> subRoutines = JsonSerializer.Deserialize<List<Routine>>(json, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                    con.Close();
                    routines.Add(item.Title, subRoutines);
                }
                int indexer = 0;
                List<CompareResult> compareresults = new List<CompareResult>();
                List<Routine> items = null;
                foreach (KeyValuePair<string, List<Routine>> kvp in routines)
                {
                    items = kvp.Value;
                    foreach (Routine item in items)
                    {
                        int i = 0;
                        foreach (KeyValuePair<string, List<Routine>> sub_kvp in routines)
                        {
                            if (i == indexer)
                                continue;
                            List<Routine> subRoutines = sub_kvp.Value;
                            Routine subItem = subRoutines.FirstOrDefault(x => x.Head == item.Head && x.Parameters == item.Parameters && x.Type == item.Type);
                            if (subItem == null)
                            {
                                if (item.Type == "function")
                                {
                                    compareresults.Add(new CompareResult
                                    {
                                        Name = item.Head,
                                        Type = item.Type,
                                        Status = item.Type + " bulunamadı",
                                        ContentLeft = "CREATE OR REPLACE FUNCTION " + item.Head + item.Parameters + Environment.NewLine + "LANGUAGE 'plpgsql' AS $BODY$" + item.Body + " $BODY$;",
                                        ContentRight = string.Empty,
                                        TitleLeft = kvp.Key,
                                        TitleRight = sub_kvp.Key
                                    });
                                }
                                else
                                {
                                    TableBody tableBody = JsonSerializer.Deserialize<TableBody>(item.Body, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                                    compareresults.Add(new CompareResult
                                    {
                                        Name = item.Head,
                                        Type = item.Type,
                                        Status = item.Type + " bulunamadı",
                                        ContentLeft = "CREATE TABLE " + item.Head + item.Parameters + Environment.NewLine + tableBody.Columns,
                                        ContentRight = string.Empty,
                                        TitleLeft = kvp.Key,
                                        TitleRight = sub_kvp.Key
                                    });
                                }
                            }
                            else
                            {
                                if (!subItem.Body.Equals(item.Body, StringComparison.OrdinalIgnoreCase))
                                {
                                    if (item.Type == "function")
                                    {
                                        compareresults.Add(new CompareResult
                                        {
                                            Name = item.Head,
                                            Type = item.Type,
                                            Status = item.Type + " içerikleri farklı",
                                            ContentLeft = "CREATE OR REPLACE FUNCTION " + item.Head + item.Parameters + Environment.NewLine + "LANGUAGE 'plpgsql' AS $BODY$" + item.Body + " $BODY$;",
                                            ContentRight = "CREATE OR REPLACE FUNCTION " + subItem.Head + subItem.Parameters + Environment.NewLine + "LANGUAGE 'plpgsql' AS $BODY$" + subItem.Body + " $BODY$;",
                                            TitleLeft = kvp.Key,
                                            TitleRight = sub_kvp.Key
                                        });
                                    }
                                    else
                                    {
                                        TableBody tableBody = JsonSerializer.Deserialize<TableBody>(item.Body, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                                        compareresults.Add(new CompareResult
                                        {
                                            Name = item.Head,
                                            Type = item.Type,
                                            Status = item.Type + " içerikleri farklı",
                                            ContentLeft = "CREATE TABLE " + item.Head + item.Parameters + Environment.NewLine + tableBody.Columns,
                                            ContentRight = "CREATE TABLE " + item.Head + item.Parameters + Environment.NewLine + tableBody.Columns,
                                            TitleLeft = kvp.Key,
                                            TitleRight = sub_kvp.Key
                                        });
                                    }
                                }
                            }
                            i++;
                        }
                    }
                    indexer++;
                }
                return compareresults;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            return null;
        }

        public List<CompareResult> GetComparesByFilter(Filter filters, List<CompareResult> results)
        {
            try
            {
                List<CompareResult> filteredResult;
                if (!string.IsNullOrEmpty(filters.Name) && filters.Type != null)
                {
                    filteredResult = results.Where(x => filters.Name == x.Name && filters.Type.Any(x.Type.Contains)).ToList();
                    return filteredResult;
                }
                else if (filters.Type != null)
                {
                    filteredResult = results.Where(x => filters.Type.Any(x.Type.Contains)).ToList();
                    return filteredResult;
                }
                else
                {
                    filteredResult = results.Where(x => filters.Name == x.Name).ToList();
                    return filteredResult;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            return null;
        }
    }
}
