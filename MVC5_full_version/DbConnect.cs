using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace MVC5_full_version
{
    public class DbConnect
    {
        SqlConnection connect = new SqlConnection();

        public DbConnect(string constring)
        {
            connect = new SqlConnection(ConfigurationManager.ConnectionStrings[constring].ConnectionString);
        }

        public DbConnect()
        {
            connect = new SqlConnection(ConfigurationManager.ConnectionStrings["conn"].ConnectionString);
        }
        public DataTable GetDataTable(string sql)
        {
            using (SqlConnection connection = GetDbConnection())
            {
                using (SqlDataAdapter da = new SqlDataAdapter(sql, connection))
                {
                    DataTable table = new DataTable();
                    da.Fill(table);
                    return table;
                }
            }
        }

        public string SelectScalar(string sql, string[] parameterNames, string[] parameterVals)
        {
            using (SqlConnection connection = GetDbConnection())
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    FillParameters(command, parameterNames, parameterVals);
                    return Convert.ToString(command.ExecuteScalar());
                }
            }
        }

        public string SelectScalar(string sql)
        {
            using (SqlConnection connection = GetDbConnection())
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    return Convert.ToString(command.ExecuteScalar());
                }
            }
        }

        public int CRUD(string sql, string[] parameterNames, string[] parameterVals)
        {
            using (SqlConnection connection = GetDbConnection())
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    FillParameters(command, parameterNames, parameterVals);
                    return command.ExecuteNonQuery();
                }
            }
        }

        public void FillParameters(SqlCommand command, string[] parameterNames, string[] parameterVals)
        {
            if (parameterNames != null)
            {
                for (int i = 0; i <= parameterNames.Length - 1; i++)
                {
                    command.Parameters.AddWithValue(parameterNames[i], parameterVals[i]);
                }
            }
        }

        public DataTable GetTable(SqlCommand command)
        {
            SqlConnection connection = GetDbConnection();

            DataTable table = new DataTable();
            command.Connection = connection;
            try
            {
                using (SqlDataAdapter da = new SqlDataAdapter(command))
                {
                    da.Fill(table);
                }
            }
            catch (Exception ex)
            { }
            finally
            {
                connection.Close();
            }
            return table;

        }

        public DataTable GetDataTable(SqlCommand command)
        {
            SqlConnection connection = GetDbConnection();

            DataTable table = new DataTable();
            command.Connection = connection;
            try
            {
                using (SqlDataAdapter da = new SqlDataAdapter(command))
                {
                    da.Fill(table);

                }
            }
            catch (Exception ex)
            { }
            finally
            {
                connection.Close();
            }
            return table;

        }

        public DataTable GetDataTable(string query, CommandType type)
        {

            DataSet daReturn = new DataSet();
            DataTable dt = new DataTable();
            SqlConnection connection = GetDbConnection();

            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.CommandType = type;

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            try
            {
                adapter.Fill(dt);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }

            }
            return dt;
        }
        public SqlConnection GetDbConnection()
        {
            try
            {
                connect = new SqlConnection(connect.ConnectionString);
                if (connect.State == ConnectionState.Closed)
                    connect.Open();
                return connect;
            }

            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}