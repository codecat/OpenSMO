using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Diagnostics;
using MySql.Data.MySqlClient; 

namespace OpenSMO
	{
	public class MySql
		{
		private MySqlConnection conn;
                public static string Host;
                public static string User;
                public static string Database;
                public static string Password;

		public MySql()
		{
			Initialize();
		}

		private void Initialize()
		{
			string connectionString;
			connectionString = "server=" + Host + ";" + "database=" + 
				Database + ";" + "userid=" + User + ";" + "password=" + Password + ";";
			
			conn = new MySqlConnection(connectionString);
		}

		private bool Open()
		{
    			try
    			{
	    	    		conn.Open();
        			return true;
    			}
 	 		catch (MySqlException ex)
    			{
			        switch (ex.Number)
        			{
            				case 0:
                				MainClass.AddLog("MySQL: Cannot connect to server.");
        	        			break;
	
				       case 1045:
	        	       			MainClass.AddLog("MySQL: Invalid username/password.");
        				        break;
        			}
       				return false;
		    	}
		} // end open

		private bool Close()
		{
			try
			{
				conn.Close();
				return true;
			}
			catch (MySqlException ex)
			{
				MainClass.AddLog(ex.ToString());
				return false;
			}
		} // end close

    		public static string AddSlashes(string str)
    		{
      			return str.Replace("'", "''");
    		} // end addslashes

		public static Hashtable[] Query(string query)
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();

			MySql obj = new MySql();
                        
			List<Hashtable> ret = new List<Hashtable>();

			if (obj.Open() == true)
			{
				MySqlCommand cmd = new MySqlCommand(query, obj.conn);
				
				MySqlDataReader reader = cmd.ExecuteReader();


      				while (reader.Read())
				{
        				Hashtable row = new Hashtable();
 				        for (int i = 0; i < reader.FieldCount; i++)
					{
          					if (reader[i].GetType() == typeof(Int64))
						{
            						if ((long)reader[i] > int.MaxValue)
            							row[reader.GetName(i)] = (long)reader[i];
            						else
              							row[reader.GetName(i)] = (int)(long)reader[i];
          					} else
            						row[reader.GetName(i)] = reader[i];
       				 	}//end for

				        ret.Add(row);
      				}//end while

			        reader.Close();
			        obj.Close();
			}//end openconnection

			sw.Stop();

		      	if (sw.ElapsedMilliseconds >= 1000)
        			MainClass.AddLog("SQL Query took very long: " + sw.ElapsedMilliseconds + "ms", true);

			      return ret.ToArray();
		}//end QueryMethod

	}//end class
} //end namespace
