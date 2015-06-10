using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace CodeAutomation
{
	public class StoredProcedure
	{
		public static void getDataSet()
		{
			var db = new APPI.Meetball.DAL.MeetballDB();
			var sp = db.GetMBInfoByHash(null);
			var ds = sp.ExecuteDataSet();

			viewContentsOfSP(ds);
		}

		/// <summary>
		/// Opens up a txt file with generated code to get the values from the dataset
		/// </summary>
		/// <param name="set"></param>
		public static void viewContentsOfSP(DataSet set)
		{
			var sb = new StringBuilder();
			for (int i = 0; i < set.Tables.Count; i++)
			{
				sb.Append("for(int i = 0; i < ds.Tables[");
				sb.Append(i);
				sb.Append("].Rows.Count; i++)");
				sb.Append(Environment.NewLine);
				sb.Append("{");
				sb.Append(Environment.NewLine);
				sb.Append("\tvar dr = ds.Tables[");
				sb.Append(i);
				sb.Append("].Rows[i];");
				sb.Append(Environment.NewLine);
				sb.Append("\tvar cn = \"\";");
				sb.Append(Environment.NewLine);

				for (int j = 0; j < set.Tables[i].Columns.Count; j++)
				{
					sb.Append("\tcn = \"");
					sb.Append(set.Tables[i].Columns[j].ColumnName);
					sb.Append("\";");
					sb.Append(Environment.NewLine);
					sb.Append("\tstring ");
					sb.Append(set.Tables[i].Columns[j].ColumnName);
					sb.Append(" = dr[cn] != dbNull ? (string)dr[cn] : null;");
					sb.Append(Environment.NewLine);
					sb.Append(Environment.NewLine);
				}
				sb.Append("}");
				sb.Append(Environment.NewLine);
			}

			var file = "C:/Users/Devin/Desktop/stuff/dataset_tmp.txt";

			File.WriteAllText(file, sb.ToString());
			Process.Start(file);
		}
	}
}
