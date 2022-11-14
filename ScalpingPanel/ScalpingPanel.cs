using cAlgo.API;
using System;
using cAlgo.API.Internals;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class ScalpingPanel : Robot
    {
        private TradingPanel tradingPanel;

        [Parameter("Vertikální umístění", Group = "Pozice panelu", DefaultValue = VerticalAlignment.Top)]
        public VerticalAlignment PanelVerticalAlignment { get; set; }

        [Parameter("Horizontální umístění", Group = "Pozice panelu", DefaultValue = HorizontalAlignment.Left)]
        public HorizontalAlignment PanelHorizontalAlignment { get; set; }

        [Parameter("Velikost pozice", Group = "Parametry pozice", DefaultValue = 0.1)]
        public double DefaultLots { get; set; }

        [Parameter("TP pipů", Group = "Parametry pozice", DefaultValue = 30)]
        public double DefaultTakeProfitPips { get; set; }

        [Parameter("SL pipů", Group = "Parametry pozice", DefaultValue = 30)]
        public double DefaultStopLossPips { get; set; }

        [Parameter("Trailing stop pipů", Group = "Trailing stop", DefaultValue = 0)]
        public double DefaultTrailingPips { get; set; }

        [Parameter("Odložený trailing stop pipů", Group = "Trailing stop", DefaultValue = 0)]
        public double DefaultTrailingDelayPips { get; set; }

        [Parameter("Obchodování z klávesnice", Group = "Obchodování z klávesnice", DefaultValue = false)]
        public bool KeyboardActive { get; set; }

        [Parameter("Klávesa pro market BUY", Group = "Obchodování z klávesnice", DefaultValue = Tradingkey.w)]
        public Tradingkey KeyMarketBuy { get; set; }

        [Parameter("Klávesa pro market SELL", Group = "Obchodování z klávesnice", DefaultValue = Tradingkey.s)]
        public Tradingkey KeyMarketSell { get; set; }

        [Parameter("Klávesa pro uzavření za MARKET", Group = "Obchodování z klávesnice", DefaultValue = Tradingkey.c)]
        public Tradingkey KeyMarketClose { get; set; }

        [Parameter("Klávesa pro zrušení čekajících pokynů", Group = "Obchodování z klávesnice", DefaultValue = Tradingkey.x)]
        public Tradingkey KeyPendingCancel { get; set; }


        protected override void OnStart()
        {
            tradingPanel = new TradingPanel(
                this, 
                Symbol, 
                DefaultLots, 
                DefaultStopLossPips, 
                DefaultTakeProfitPips, 
                KeyboardActive,
                DefaultTrailingPips,
                DefaultTrailingDelayPips,
                KeyMarketBuy, 
                KeyMarketSell, 
                KeyMarketClose, 
                KeyPendingCancel
            );

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
            
            string targetS = tradingPanel.trailingPipsInput.Text;
            if (string.IsNullOrEmpty(targetS))
                targetS = "0";
            
            double target = Convert.ToDouble(targetS.Replace(".", ","));
            if (target < 0)
                target = 0;

            string targetDelayS = tradingPanel.trailingDelayPipsInput.Text;
            if (string.IsNullOrEmpty(targetDelayS))
                targetDelayS = "0";

            double targetDelay = Convert.ToDouble(targetDelayS.Replace(".", ","));
            if (targetDelay < 0)
                targetDelay = 0;

            if (target > 0)
            {
                foreach (var position in Positions)
                {
                    if (position.SymbolName == Symbol.Name)
                    {
                        double entry = position.EntryPrice;
                        double stoploss = (double)position.StopLoss;
                        
                        // Short
                        if (position.TradeType == TradeType.Sell)
                        {
                            if (Symbol.Ask < (entry - target - targetDelay))
                            {
                                if (position.StopLoss - Symbol.Ask > target)
                                {
                                    position.ModifyStopLossPrice(Symbol.Ask + target);
                                }
                            }
                        }

                        // long
                        if (position.TradeType == TradeType.Buy)
                        {
                            if (Symbol.Bid > (target + entry + targetDelay))
                            {
                                Print((Symbol.Bid - position.StopLoss).ToString() + "<" + target.ToString());
                                if (Symbol.Bid - position.StopLoss > target)
                                {
                                    position.ModifyStopLossPrice(Symbol.Bid - target);
                                }
                            }
                        }
                    }
                }
            }




            var today = Server.Time;
            var from = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0);
            var to = new DateTime(today.Year, today.Month, today.Day, 23, 59, 59);
            double todayTotal = 0;
            foreach (HistoricalTrade trade in History)
            {
                if(trade.ClosingTime >= from && trade.ClosingTime < to)
                {
                    todayTotal = todayTotal + trade.NetProfit;
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
