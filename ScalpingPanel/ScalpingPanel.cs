using cAlgo.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using cAlgo.API.Internals;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class ScalpingPanel : Robot
    {
        private TradingPanel tradingPanel;

        [Parameter("Vertikální umístění", Group = "Panel alignment", DefaultValue = VerticalAlignment.Top)]
        public VerticalAlignment PanelVerticalAlignment { get; set; }

        [Parameter("Horizontální umístění", Group = "Panel alignment", DefaultValue = HorizontalAlignment.Left)]
        public HorizontalAlignment PanelHorizontalAlignment { get; set; }

        [Parameter("Velikost pozice", Group = "Default trade parameters", DefaultValue = 0.1)]
        public double DefaultLots { get; set; }

        [Parameter("TP pipů", Group = "Default trade parameters", DefaultValue = 300)]
        public double DefaultTakeProfitPips { get; set; }

        [Parameter("SL pipů", Group = "Default trade parameters", DefaultValue = 300)]
        public double DefaultStopLossPips { get; set; }

        [Parameter("Obchodování z klávesnice", Group = "Default trade parameters", DefaultValue = false)]
        public bool KeyboardActive { get; set; }


        protected override void OnStart()
        {
            tradingPanel = new TradingPanel(this, Symbol, DefaultLots, DefaultStopLossPips, DefaultTakeProfitPips, KeyboardActive);

            var border = new Border
            {
                VerticalAlignment = PanelVerticalAlignment,
                HorizontalAlignment = PanelHorizontalAlignment,
                Style = Styles.CreatePanelBackgroundStyle(),
                Margin = "20 40 20 20",
                Width = 180,
                Child = tradingPanel
            };

            Chart.AddControl(border);
        }

        protected override void OnTick()
        {
            var today = Server.Time;
            var from = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0);
            var to = new DateTime(today.Year, today.Month, today.Day, 23, 59, 59);
            double todayTotal = 0;
            foreach (HistoricalTrade trade in History)
            {
                if(trade.ClosingTime >= from && trade.ClosingTime < to)
                {
                    todayTotal = todayTotal + trade.GrossProfit;
                }
            }

            tradingPanel.todayTotal.Text = Math.Round(todayTotal, 2).ToString();
            if (todayTotal > 0)
                tradingPanel.todayTotal.ForegroundColor = Color.Green;
            if (todayTotal < 0)
                tradingPanel.todayTotal.ForegroundColor = Color.Red;


            double percent = 100 * todayTotal / Account.Balance;
            tradingPanel.todayTotalPercent.Text = Math.Round(percent, 2).ToString() + "%";
            if (percent > 0)
                tradingPanel.todayTotalPercent.ForegroundColor = Color.Green;
            if (percent < 0)
                tradingPanel.todayTotalPercent.ForegroundColor = Color.Red;
            
        }
    }
}
