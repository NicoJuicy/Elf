﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Configuration;
using LinqToDB.Data;
using Microsoft.Data.SqlClient;

namespace Elf.Services.Entities
{
    public partial class AppDataConnection : DataConnection
    {
        public ITable<Link> Link => GetTable<Link>();

        public ITable<LinkTracking> LinkTracking => GetTable<LinkTracking>();

        public AppDataConnection(LinqToDbConnectionOptions<AppDataConnection> options)
            : base(options)
        {
            InitDataContext();
            InitMappingSchema();
        }

        partial void InitDataContext();
        partial void InitMappingSchema();

        public bool IsFirstRun()
        {
            if (Connection.State == System.Data.ConnectionState.Closed)
            {
                Connection.Open();
            }

            var sql = "SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'Link'";

            using var cmd = new SqlCommand(sql, Connection as SqlConnection);
            var result = cmd.ExecuteScalar();
            Connection.Close();

            var tableExists = result is not null && (int)result == 1;
            return !tableExists;
        }

        public bool TestDatabaseConnection(Action<Exception> errorLogAction = null)
        {
            try
            {
                if (Connection.State == System.Data.ConnectionState.Closed)
                {
                    Connection.Open();
                }

                using var cmd = new SqlCommand("SELECT 1", Connection as SqlConnection);
                var result = cmd.ExecuteScalar();
                Connection.Close();

                return result is not null;
            }
            catch (Exception e)
            {
                errorLogAction?.Invoke(e);
                return false;
            }
        }

        public void SetupDatabase()
        {
            var sql = GetEmbeddedSqlScript("schema-mssql-140");
            if (!string.IsNullOrWhiteSpace(sql))
            {
                if (Connection.State == System.Data.ConnectionState.Closed)
                {
                    Connection.Open();
                }

                using var cmd = new SqlCommand(sql, Connection as SqlConnection);
                cmd.ExecuteNonQuery();

                Connection.Close();
            }
        }

        private static string GetEmbeddedSqlScript(string scriptName)
        {
            var assembly = typeof(AppDataConnection).GetTypeInfo().Assembly;
            using var stream = assembly.GetManifestResourceStream($"Elf.Services.DbSchema.{scriptName}.sql");
            using var reader = new StreamReader(stream);
            var sql = reader.ReadToEnd();
            return sql;
        }
    }

    public static partial class TableExtensions
    {
        public static Link Find(this ITable<Link> table, int Id)
        {
            return table.FirstOrDefault(t =>
                t.Id == Id);
        }

        public static Task<Link> FindAsync(this ITable<Link> table, int Id)
        {
            return table.FirstOrDefaultAsync(t =>
                t.Id == Id);
        }

        public static LinkTracking Find(this ITable<LinkTracking> table, Guid Id)
        {
            return table.FirstOrDefault(t =>
                t.Id == Id);
        }

        public static Task<LinkTracking> FindAsync(this ITable<LinkTracking> table, Guid Id)
        {
            return table.FirstOrDefaultAsync(t =>
                t.Id == Id);
        }
    }
}
