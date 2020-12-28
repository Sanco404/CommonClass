using System;
using System.Windows.Forms;
using DataConnectionDialog = Microsoft.Data.ConnectionUI.DataConnectionDialog;
using DataProvider = Microsoft.Data.ConnectionUI.DataProvider;
using DataSource = Microsoft.Data.ConnectionUI.DataSource;

namespace ConnectionPromptDialog
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void btn_DataConnectDialog_Click(object sender, EventArgs e)
        {
            this.tb_ConnectionStr.Text = GetDatabaseConnectionString();
        }

        private string GetDatabaseConnectionString()
        {
            string result = string.Empty;
            DataConnectionDialog dialog = new DataConnectionDialog();
            dialog.DataSources.Clear();

            //添加数据源列表，可以向窗口中添加所需要的数据源类型 必须至少有一项
            dialog.DataSources.Add(DataSource.AccessDataSource);    //Access
            dialog.DataSources.Add(DataSource.SqlDataSource);       //Sql Server
            dialog.DataSources.Add(DataSource.OracleDataSource);    //Oracle
            dialog.DataSources.Add(DataSource.OdbcDataSource);      //Odbc
            dialog.DataSources.Add(DataSource.SqlFileDataSource);   //Sql Server File

            //设置默认数据提供程序
            dialog.SelectedDataSource = DataSource.SqlDataSource;
            dialog.SelectedDataProvider = DataProvider.SqlDataProvider;

            //本文地址：http://www.cnblogs.com/Interkey/p/DataConnectionDialog.html
            //dialog.Title = "Cosmic_Spy";
            //dialog.ConnectionString = "Data Source=MAPF-PC;Initial Catalog=NoYes;Integrated Security=True"; //也可以设置默认连接字符串
            //只能够通过DataConnectionDialog类的静态方法Show出对话框，不能使用dialog.Show()或dialog.ShowDialog()来呈现对话框
            if (DataConnectionDialog.Show(dialog) == DialogResult.OK)
            {
                result = dialog.ConnectionString;
            }
            return result;
        }
    }
}
