﻿using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace SeSecEL.library
{
    public class Tools : Constantes
    { 
        public const int TimeOut = 1200;
        public long ExeccuteCommand(string pagina, StringBuilder strSQL)
        {
            SqlConnection cnComando = OpenSQLConnection(pagina);
            long Rows = 0; 

            if (cnComando != null)
            {
                try
                {
                    SqlTransaction trComando;
                    SqlCommand cmComando = new SqlCommand(strSQL.ToString(), cnComando);
                    trComando = cnComando.BeginTransaction();

                    cmComando.CommandTimeout = TimeOut;
                    cmComando.Transaction = trComando;
                    cmComando.CommandType = CommandType.Text;

                    Rows = cmComando.ExecuteNonQuery();
                    trComando.Commit();
                    trComando.Dispose();
                    cmComando.Dispose();
                    cmComando = null;
                }
                catch (Exception ex)
                {
                    Rows = 0; 
                    WriteToFile(ex.Message);

                }
                finally
                {
                    cnComando.Close();
                }
            }
            return Math.Abs(Rows);
        } // End ExecCommand 

        private string GetConnection() => ConfigurationManager.AppSettings["StringConection"];
        public SqlConnection OpenSQLConnection(string Pagina)
        {
            SqlConnection cnSQL = new SqlConnection { 
                ConnectionString = GetConnection() 
            }; 
            try
            {
                cnSQL.Open();
            }
            catch (Exception ex)
            {
                WriteToFile(ex.Message); 
                cnSQL = null;
            }
            return cnSQL;
        }
        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }

        /// <summary>
        /// TEst the connection or, 
        /// if fail, ten end the progam
        /// </summary>
        /// <param name="pagina"></param>
        /// <returns></returns>
        public SqlConnection TestConnection(string pagina)
        {
            SqlConnection cnSQL = null;
            cnSQL = OpenSQLConnection(pagina);
            return cnSQL;
        }

        public SqlDataReader OpenDataReader(string pagina, string funcion, StringBuilder strSQL)
        {
            SqlConnection cn = null;
            SqlCommand cm = null;
            cn = OpenSQLConnection(pagina);
            SqlDataReader drSQL = null;
            if (cn != null)
            {
                try
                {
                    cm = new SqlCommand 
                    { 
                        CommandText = strSQL.ToString(), 
                        Connection = cn, 
                        CommandTimeout = TimeOut 
                    };
                    drSQL = cm.ExecuteReader();
                }
                catch (Exception ex)
                { 
                    WriteToFile(ex.Message);
                }
            }
            return drSQL;
        }
        public DataTable FillDT(string pagina, string funcion, StringBuilder strSQL)
        {
            using (DataTable tbl = new DataTable("consulta"))
            {
                try
                {
                    SqlConnection cnDataSet = OpenSQLConnection(pagina);
                    if (cnDataSet != null)
                    {
                        using (SqlDataAdapter daDataSet = new SqlDataAdapter(strSQL.ToString(), cnDataSet))
                        {
                            daDataSet.Fill(tbl);
                        }
                        cnDataSet.Close();
                        cnDataSet = null;
                    }
                    else
                    {
                        tbl.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    WriteToFile(ex.Message); 
                    return null;
                }
                return tbl;
            }

        }

        private bool GetUpperCase
        {
            get
            {
                bool.TryParse(ConfigurationManager.AppSettings["UpperCase"], out bool bValor);
                return bValor;
            }
        }
        public string ID(string forma, string strQuery)
        {
            SqlConnection cnClave = OpenSQLConnection(forma);
            string strValor = "";
            if (cnClave != null)
            {
                SqlDataReader dr;
                try
                {
                    SqlCommand cm = new SqlCommand {
                        Connection = cnClave, 
                        CommandText = strQuery, 
                        CommandTimeout = TimeOut 
                    };
                    dr = cm.ExecuteReader();
                    if (dr.Read())
                    {
                        if (dr[0].GetType().FullName == "System.DateTime")
                        {
                            if (dr[0].ToString().Length != 0)
                            {
                                strValor = Convert.ToDateTime(dr[0].ToString()).ToString("yyyy-MM-dd");
                            }
                        }
                        else
                        {
                            strValor = dr[0].ToString().Trim();
                        }
                    }
                    dr.Close();
                    cm.Dispose();
                    cm = null;
                    dr = null;
                }
                catch (Exception ex)
                {
                    strValor = "";
                    WriteToFile(ex.Message + " " + strQuery);
                }
                cnClave.Close();
            }
            return strValor;
        }
    }
}
