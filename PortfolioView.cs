using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.Json;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.IO;

namespace CryptoPortfolioManager
{
    public partial class PortfolioView : UserControl
    {
        const string GET_MARKET = "https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&order=market_cap_desc";
        static List<CoinViewItem> ViewItems = new List<CoinViewItem>();
        PopupForm frmPopup = null;
        static PortfolioView Instance = null;
        public PortfolioView()
        {
            Instance = this;
            InitializeComponent();
            Data.WaitComponentList.Add(btnAddSymbol);
            Data.Init();
        }
        private void btnAddSymbol_Click(object sender, EventArgs e)
        {
            if (frmPopup == null)
            {
                frmPopup = new PopupForm();
                frmPopup.Add = Add;
            }

            frmPopup.ShowDialog();
        }
        private void Add()
        {
            int index = frmPopup.cmbSymbol.SelectedIndex;
            string coinId = Data.Response[index].id;
            float addAmount = float.Parse(frmPopup.txtInput.Text);
            int displayIndex = Data.DisplayData.FindIndex(d => d.CoinId == coinId);
            if(displayIndex == -1)
            {
                ToDisplay datum = new ToDisplay
                {
                    MarketIndex = index,
                    Amount = addAmount,
                    CoinId = coinId
                };
                Data.DisplayData.Add(datum);
                Data.UpdateData(datum, true);
            }
            else
            {
                Data.DisplayData[displayIndex].Amount += addAmount;
                Data.UpdateData(Data.DisplayData[displayIndex], false);
                if (Data.DisplayData[displayIndex].Amount <= 0)
                {
                    Data.DisplayData.RemoveAt(displayIndex);
                }
            }
            RefreshAndDisplay();
        }
        public static void RefreshAndDisplay()
        {
            Data.Refresh();
            Instance.lblTotal.Text = $"Total Portfolio Value : ${ToDisplay.total}";
            int needViews = Data.DisplayData.Count - ViewItems.Count;
            if(needViews > 0)
            {
                for (int i = 0; i < needViews; i++)
                {
                    ViewItems.Add(new CoinViewItem(Instance?.tableContainer));
                }
            } else if (needViews < 0)
            {
                CoinViewItem.Pop(Instance?.tableContainer);
                ViewItems.RemoveAt(ViewItems.Count - 1); ;
            }
            
            for(int i = 0; i < Data.DisplayData.Count; i++)
            {
                ViewItems[i].SetData(Data.DisplayData[i]);
            }
        }
        private class PopupForm : Form
        {
            internal List<Coin> coinData = null;
            internal Action Add;
            internal ComboBox cmbSymbol = null;
            internal TextBox txtInput = null;
            Label lblSymbol = null;
            internal PopupForm()
            {
                cmbSymbol = new ComboBox();
                txtInput = new TextBox();
                lblSymbol = new Label();

                Button _btnAdd = new Button();
                Button _btnCancel = new Button();
                Label _lbl1 = new Label();
                Label _lbl2 = new Label();

                SuspendLayout();

                _lbl1.Text = "Select Symbol :";
                _lbl1.Location = new Point(12, 16);

                cmbSymbol.Size = new Size(121, 28);
                cmbSymbol.Location = new Point(138, 12);
                cmbSymbol.DropDownStyle = ComboBoxStyle.DropDownList;
                LoadList();
                cmbSymbol.SelectedIndexChanged += (o, e) => Change();

                _lbl2.Text = "Input Value :";
                _lbl2.Location = new Point(12, 61);

                txtInput.Location = new Point(138, 58);
                txtInput.Size = new Size(64, 26);

                lblSymbol.Text = "";
                lblSymbol.Location = new Point(208, 61);
                lblSymbol.Size = new Size(64, 26);

                _btnCancel.Text = "Cancel";
                _btnCancel.Size = new Size(90, 33);
                _btnCancel.Location = new Point(154, 108);
                _btnCancel.Click += (o, e) => Close();

                _btnAdd.Text = "Add";
                _btnAdd.Size = new Size(90, 33);
                _btnAdd.Location = new Point(41, 108);
                _btnAdd.Click += (o, e) => TextValidate();

                Text = "Add Symbol";
                ClientSize = new Size(290, 150);
                FormBorderStyle = FormBorderStyle.FixedDialog;
                StartPosition = FormStartPosition.CenterScreen;
                Controls.AddRange(new Control[]
                {
                    cmbSymbol,
                    txtInput,
                    _lbl1,
                    _lbl2,
                    lblSymbol,
                    _btnAdd,
                    _btnCancel
                });
                ResumeLayout();
            }
            private void TextValidate()
            {
                if (Regex.IsMatch(txtInput.Text, @"^-?[0-9]+\.?[0-9]*$"))
                {
                    Add?.Invoke();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Please correct value.", "Warining");
                }
            }
            private void Change()
            {
                lblSymbol.Text = Data.Response[cmbSymbol.SelectedIndex].symbol;
            }
            private void LoadList()
            {
                if (cmbSymbol == null || Data.Response.Count == 0) return;
                cmbSymbol.Items.Clear();
                Data.Response.ForEach(_coin => cmbSymbol.Items.Add(_coin.name));
                cmbSymbol.SelectedIndex = 0;
            }
        }
        private class CoinViewItem : TableLayoutPanel
        {
            PictureBox picture;
            Label lblSymbol, lblPrice, lblChangePercent, lblValue, lblPercent, lblAmount, lblDifference;
            ProgressBar view;
            internal CoinViewItem(TableLayoutPanel parent)
            {
                picture = new PictureBox();
                lblSymbol = new Label();
                lblPrice = new Label();
                lblChangePercent = new Label();
                lblValue = new Label();
                lblPercent = new Label();
                lblAmount = new Label();
                lblDifference = new Label();

                TableLayoutPanel _symbolPanel = new TableLayoutPanel();
                view = new ProgressBar();

                parent.SuspendLayout();
                SuspendLayout();
                _symbolPanel.SuspendLayout();

                parent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                parent.Controls.Add(this);
                parent.RowCount = parent.Controls.Count;
                parent.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));

                ColumnCount = 7;
                ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
                ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
                ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
                ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
                ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
                ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
                ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
                Controls.Add(picture, 0, 0);
                Controls.Add(_symbolPanel, 1, 0);
                Controls.Add(lblValue, 2, 0);
                Controls.Add(lblPercent, 3, 0);
                Controls.Add(lblAmount, 4, 0);
                Controls.Add(lblDifference, 5, 0);
                Controls.Add(view, 6, 0);
                Dock = DockStyle.Fill;
                RowCount = 1;
                RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));

                picture.SizeMode = PictureBoxSizeMode.Zoom;

                _symbolPanel.ColumnCount = 1;
                _symbolPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
                _symbolPanel.Controls.Add(lblSymbol);
                _symbolPanel.Controls.Add(lblPrice);
                _symbolPanel.Controls.Add(lblChangePercent);
                _symbolPanel.Dock = DockStyle.Fill;
                _symbolPanel.RowCount = 3;
                _symbolPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
                _symbolPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
                _symbolPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));

                lblSymbol.Dock = DockStyle.Fill;
                lblSymbol.TextAlign = ContentAlignment.MiddleLeft;

                lblPrice.Dock = DockStyle.Fill;
                lblPrice.TextAlign = ContentAlignment.MiddleLeft;

                lblChangePercent.Dock = DockStyle.Fill;
                lblChangePercent.TextAlign = ContentAlignment.MiddleLeft;

                lblValue.Dock = DockStyle.Fill;
                lblValue.TextAlign = ContentAlignment.MiddleCenter;

                lblPercent.Dock = DockStyle.Fill;
                lblPercent.TextAlign = ContentAlignment.MiddleCenter;

                lblAmount.Dock = DockStyle.Fill;
                lblAmount.TextAlign = ContentAlignment.MiddleCenter;

                lblDifference.Dock = DockStyle.Fill;
                lblDifference.TextAlign = ContentAlignment.MiddleCenter;

                view.Dock = DockStyle.Fill;
                view.Margin = new Padding(5, 15, 5, 15);
                view.Value = 50;

                parent.ResumeLayout(false);
                ResumeLayout(false);
                _symbolPanel.ResumeLayout(false);
                _symbolPanel.PerformLayout();
                Parent.PerformLayout();
            }

            internal static void Pop(TableLayoutPanel parent)
            {
                parent.SuspendLayout();
                parent.Controls.RemoveAt(parent.Controls.Count - 1);
                parent.RowStyles.RemoveAt(parent.RowStyles.Count - 1);
                parent.RowCount--;
                parent.ResumeLayout(false);
                parent.PerformLayout();
            }

            internal void SetData(ToDisplay datum)
            {
                picture.ImageLocation = Data.Response[datum.MarketIndex].image;
                lblSymbol.Text = Data.Response[datum.MarketIndex].name;
                lblPrice.Text = Data.Response[datum.MarketIndex].current_price.ToString("$0.00");
                lblChangePercent.Text = datum.ChangePercent.ToString("0.00%");
                lblChangePercent.ForeColor = (datum.ChangePercent >= 0) ? Color.Green : Color.Red;
                lblValue.Text = datum.Value.ToString("$0.00");
                lblPercent.Text = datum.Percentage.ToString("0.00%");
                lblAmount.Text = datum.Amount.ToString("0.00 ") + Data.Response[datum.MarketIndex].symbol;
                lblDifference.Text = datum.Difference.ToString("$0.00");
                lblDifference.ForeColor = (datum.Difference >= 0) ? Color.Green : Color.Red;
                view.Value = Math.Max(0, Math.Min((int)datum.View, 100));
            }
        }
        private class Coin
        {
            public string id { get; set; }
            public string symbol { get; set; }
            public string name { get; set; }
            public string image { get; set; }
            public float current_price { get; set; }
            public float price_change_24h { get; set; }
        }
        private class ToDisplay
        {
            internal static float total = 0;
            internal static float max = 0;

            private float _amount = 0;
            internal string CoinId { get; set; }
            internal int MarketIndex { get; set; }

            // Need to preset MarketIndex and Data.Response
            internal float Amount
            {
                get => _amount;
                set
                {
                    _amount = value;
                    Value = value * Data.Response[MarketIndex].current_price;
                }
            }
            internal float Value { get; private set; }
            internal float ChangePercent { get; private set; }
            internal float Percentage { get; private set; }
            internal float Difference { get; private set; }
            internal float View { get; private set; }

            // Need to preset MarketIndex, Data.Response, max, total
            internal void Calc()
            {
                Coin coin  = Data.Response[MarketIndex];
                ChangePercent = coin.price_change_24h / coin.current_price;
                Percentage = (total > 0) ? Value / total : 0;
                Difference = Amount * coin.price_change_24h;
                View = (max > 0) ? Value * 100f / max : 0;
            }
        }
        private class Data: IDisposable
        {
            static internal List<Coin> Response = new List<Coin>();
            static internal List<ToDisplay> DisplayData = new List<ToDisplay>();
            static internal List<Control> WaitComponentList = new List<Control>();
            static string strDBPath = "database.db";

            static SQLiteConnection sqlite_conn = null;
            static internal async void Init()
            {
                Wait(false);
                await GetMarketData();
                ReadData();
                Wait(true);
                RefreshAndDisplay();
            }
            static public async Task GetMarketData()
            {
                using (var client = new HttpClient())
                {
                    HttpResponseMessage response;
                    response = await client.GetAsync(GET_MARKET, HttpCompletionOption.ResponseContentRead);
                    string res = await response.Content.ReadAsStringAsync();
                    Response = JsonSerializer.Deserialize<List<Coin>>(res);
                }
            }
            static void OpenConnection()
            {
                if (sqlite_conn?.State == System.Data.ConnectionState.Open) return;
                try {
                    sqlite_conn = new SQLiteConnection($"Data Source={strDBPath};Version=3;New=True;Compress=True;");
                    sqlite_conn.Open();
                    CreateTable();
                }
                catch(Exception ex) {
                    Console.WriteLine(ex.Message);
                }
            }
            static void CreateTable()
            {
                SQLiteCommand sqlite_cmd;
                string Createsql = "CREATE TABLE IF NOT EXISTS PosData (id VARCHAR(20) PRIMARY KEY, pos Single)";
                sqlite_cmd = sqlite_conn.CreateCommand();
                sqlite_cmd.CommandText = Createsql;
                sqlite_cmd.ExecuteNonQuery();
            }
            static internal void UpdateData(ToDisplay datum, bool isNew)
            {
                OpenConnection();
                SQLiteCommand sqlite_cmd;
                sqlite_cmd = sqlite_conn.CreateCommand();
                sqlite_cmd.CommandText = $"INSERT OR REPLACE INTO PosData ('id', 'pos') VALUES('{datum.CoinId}', {datum.Amount});";
                sqlite_cmd.ExecuteNonQuery();
            }
            static void ReadData()
            {
                OpenConnection();
                SQLiteDataReader sqlite_datareader;
                SQLiteCommand sqlite_cmd;
                sqlite_cmd = sqlite_conn.CreateCommand();
                sqlite_cmd.CommandText = "SELECT * FROM PosData";

                sqlite_datareader = sqlite_cmd.ExecuteReader();
                while (sqlite_datareader.Read())
                {
                    string id = sqlite_datareader.GetString(0);
                    ToDisplay datum = new ToDisplay
                    {
                        MarketIndex = Data.Response.FindIndex(r => r.id == id),
                        CoinId = id,
                        Amount = float.Parse(sqlite_datareader.GetValue(1).ToString())
                    };
                    if (datum.Amount > 0)
                    {
                        DisplayData.Add(datum);
                    }
                }
            }
            private static void Wait(bool isFree)
            {
                if (WaitComponentList.Count == 0) return;
                WaitComponentList.ForEach(c => c.Enabled = isFree);
            }
            public void Dispose()
            {
                sqlite_conn.Close();
            }

            internal static void Refresh()
            {
                float _max = 0;
                float _total = 0;
                DisplayData.Sort((x, y) => x.Value.CompareTo(y.Value) * -1);
                DisplayData.ForEach(d =>
                {
                    if (d.Value > _max) _max = d.Value;
                    _total += d.Value;
                });
                ToDisplay.total = _total;
                ToDisplay.max = _max;
                DisplayData.ForEach(d => d.Calc());
            }
        }

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            await Data.GetMarketData();
            Data.DisplayData.ForEach(d => d.Amount = d.Amount);
            RefreshAndDisplay();
        }
    }
}
