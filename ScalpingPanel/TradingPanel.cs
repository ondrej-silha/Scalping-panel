using cAlgo.API.Internals;
using cAlgo.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;

namespace cAlgo
{
    public class TradingPanel : CustomControl
    {
        private const int actionBuyMarket = 0;
        private const int actionSellMarket = 1;
        private const int actionBuyPending = 2;
        private const int actionSellPending = 3;
        private const int actionClosePending = 4;
        private const int actionCloseOpen = 5;

        private int pendingTradeType;
        private bool pendingTradeOpen = false;
        private Button pendingButton;
        public TextBlock todayTotal;
        public TextBlock todayTotalPercent;
        public TextBlock todayActual;



        private const string LotsInputKey = "LotsKey";
        private const string TakeProfitInputKey = "TPKey";
        private const string StopLossInputKey = "SLKey";
        private readonly IDictionary<string, TextBox> _inputMap = new Dictionary<string, TextBox>();
        private readonly Robot _robot;
        private readonly Symbol _symbol;

        public TradingPanel(Robot robot, Symbol symbol, double defaultLots, double defaultStopLossPips, double defaultTakeProfitPips)
        {
            _robot = robot;
            _symbol = symbol;
            AddChild(CreateTradingPanel(defaultLots, defaultStopLossPips, defaultTakeProfitPips));
            _robot.Chart.MouseDown += Chart_MouseDown;
            _robot.Chart.KeyDown += Chart_KeyDown;
        }

        private void Chart_KeyDown(ChartKeyboardEventArgs obj)
        {
            if (obj.Key == Key.NumPad0)
            {
                CloseAllOpen();
            }
            if (obj.Key == Key.Decimal)
            {
                CloseAllPending();
            }
            if (obj.Key == Key.NumPad4)
            {
                MarketBuy();
            }
            if (obj.Key == Key.NumPad6)
            {
                MarketSell();
            }
        }

        private ControlBase CreateTradingPanel(double defaultLots, double defaultStopLossPips, double defaultTakeProfitPips)
        {
            var mainPanel = new StackPanel();

            var header = CreateHeader();
            mainPanel.AddChild(header);

            var contentPanel = CreateContentPanel(defaultLots, defaultStopLossPips, defaultTakeProfitPips);
            mainPanel.AddChild(contentPanel);

            return mainPanel;
        }

        private ControlBase CreateHeader()
        {
            var headerBorder = new Border
            {
                BorderThickness = "0 0 0 1",
                Style = Styles.CreateCommonBorderStyle()
            };

            var header = new TextBlock
            {
                Text = "Scalping panel",
                Margin = "10 7",
                Style = Styles.CreateHeaderStyle()
            };

            headerBorder.Child = header;
            return headerBorder;
        }

        private StackPanel CreateContentPanel(double defaultLots, double defaultStopLossPips, double defaultTakeProfitPips)
        {
            var contentPanel = new StackPanel
            {
                Margin = 10
            };
            var grid = new Grid(19, 3);
            grid.Columns[1].SetWidthInPixels(5);

            var buyButton = CreateButton("M. BUY", Styles.CreateBuyButtonStyle(), actionBuyMarket);
            grid.AddChild(buyButton, 0, 0);

            var sellButton = CreateButton("M. SELL", Styles.CreateSellButtonStyle(), actionSellMarket);
            grid.AddChild(sellButton, 0, 2);
            grid.Rows[1].SetHeightInPixels(5);

            var pendingBuyButton = CreateButton("P. BUY", Styles.CreateBuyButtonStyle(), actionBuyPending);
            grid.AddChild(pendingBuyButton, 2, 0);

            var pendingSellButton = CreateButton("P. SELL", Styles.CreateSellButtonStyle(), actionSellPending);
            grid.AddChild(pendingSellButton, 2, 2);
            grid.Rows[3].SetHeightInPixels(5);

            var closePendingButton = CreateButton("Zavřít všechny čekající", Styles.CreateCloseButtonStyle(), actionClosePending);
            grid.AddChild(closePendingButton, 4, 0, 1, 3);

            grid.Rows[5].SetHeightInPixels(5);

            var closeAllButton = CreateButton("Zavřít všechny otevřené", Styles.CreateCloseButtonStyle(), actionCloseOpen);
            grid.AddChild(closeAllButton, 6, 0, 1, 3);

            grid.Rows[7].SetHeightInPixels(8);

            var lotsInput = CreateInputWithLabel("Velikost pozice", defaultLots.ToString("F2"), LotsInputKey);
            grid.AddChild(lotsInput, 8, 0);

            grid.Rows[9].SetHeightInPixels(8);
            var takeProfitInput = CreateInputWithLabel("TP (pipů)", defaultTakeProfitPips.ToString("F1"), TakeProfitInputKey);
            grid.AddChild(takeProfitInput, 10, 0);

            var stopLossInput = CreateInputWithLabel("SL (pipů)", defaultStopLossPips.ToString("F1"), StopLossInputKey);
            grid.AddChild(stopLossInput, 10, 2);
            grid.Rows[11].SetHeightInPixels(8);

            var totalOpenLabel = CreateLabel("Zisk dnes");
            grid.AddChild(totalOpenLabel, 12, 0);

            var totalOpenPercentLabel = CreateLabel("Zisk dnes %");
            grid.AddChild(totalOpenPercentLabel, 12, 2);
            grid.Rows[13].SetHeightInPixels(5);

            var totalOpen = CreateLabeTodayTotal("0");
            grid.AddChild(totalOpen, 14, 0);

            var totalOpenPercent = CreateLabeTodayTotalPercent("0");
            grid.AddChild(totalOpenPercent, 14, 2);
            grid.Rows[15].SetHeightInPixels(5);

            var actualEarnLabel = CreateLabel("Aktuálně");
            grid.AddChild(actualEarnLabel, 16, 0);
            grid.Rows[17].SetHeightInPixels(5);

            var actual = CreateLabelActual("0");
            grid.AddChild(actual, 18, 0);

            contentPanel.AddChild(grid);

            return contentPanel;
        }

       
        private Button CreateButton(string text, Style style, int actionType)
        {
            var button = new Button
            {
                Text = text,
                Style = style,
                Height = 25,
            };
            

            button.Click += args => Button_Click(actionType, button);
            return button;
        }

        private void Button_Click(int actionType, Button button)
        {

            if (actionType == actionBuyMarket)
            {
                MarketBuy();
            }
            if (actionType == actionSellMarket)
            {
                MarketSell();
            }
            if (actionType == actionBuyPending)
            {
                this.pendingTradeType = actionType;
                this.pendingTradeOpen = true;
                button.Text = "Umístit ...";
                pendingButton = button;
            }

            if (actionType == actionSellPending)
            {
                this.pendingTradeType = actionType;
                this.pendingTradeOpen = true;
                button.Text = "Umístit ...";
                pendingButton = button;
            }

            if(actionType == actionCloseOpen)
            {
                this.CloseAllOpen();
            }

            if(actionType == actionClosePending)
            {
                CloseAllPending();
            }
        }

        private void MarketBuy()
        {
            var lots = GetValueFromInput(LotsInputKey, 0);
            var stopLossPips = GetValueFromInput(StopLossInputKey, 0);
            var takeProfitPips = GetValueFromInput(TakeProfitInputKey, 0);
            var volume = _symbol.QuantityToVolumeInUnits(lots);
            _robot.ExecuteMarketOrderAsync(TradeType.Buy, _symbol.Name, volume, "ScalpingPanel - buy market", stopLossPips, takeProfitPips);
        }

        private void MarketSell()
        {
            var lots = GetValueFromInput(LotsInputKey, 0);
            var stopLossPips = GetValueFromInput(StopLossInputKey, 0);
            var takeProfitPips = GetValueFromInput(TakeProfitInputKey, 0);
            var volume = _symbol.QuantityToVolumeInUnits(lots);
            _robot.ExecuteMarketOrderAsync(TradeType.Sell, _symbol.Name, volume, "ScalpingPanel - sell market", stopLossPips, takeProfitPips);
        }
        private void Chart_MouseDown(ChartMouseEventArgs obj)
        {
            double clickPrice = obj.YValue;
            if(pendingTradeOpen)
            {
                if (pendingTradeType == actionBuyPending)
                {
                    var lots = GetValueFromInput(LotsInputKey, 0);
                    var stopLossPips = GetValueFromInput(StopLossInputKey, 0);
                    var takeProfitPips = GetValueFromInput(TakeProfitInputKey, 0);
                    var volume = _symbol.QuantityToVolumeInUnits(lots);
                    double ask = _robot.Symbol.Ask;
                    if (ask > clickPrice)
                    {
                        // buy limit
                        _robot.PlaceLimitOrderAsync(TradeType.Buy, _symbol.Name, volume, clickPrice, "ScalpingPanel - buy limit", stopLossPips, takeProfitPips);
                    }
                    if (ask < clickPrice)
                    {
                        // buy stop
                        _robot.PlaceStopOrderAsync(TradeType.Buy, _symbol.Name, volume, clickPrice, "ScalpingPanel - buy stop", stopLossPips, takeProfitPips);
                    }

                    pendingButton.Text = "P. BUY";
                    pendingTradeOpen = false;
                }

                if (pendingTradeType == actionSellPending)
                {
                    var lots = GetValueFromInput(LotsInputKey, 0);
                    var stopLossPips = GetValueFromInput(StopLossInputKey, 0);
                    var takeProfitPips = GetValueFromInput(TakeProfitInputKey, 0);
                    var volume = _symbol.QuantityToVolumeInUnits(lots);
                    double ask = _robot.Symbol.Ask;
                    if (ask < clickPrice)
                    {
                        // sell stop
                        _robot.PlaceLimitOrderAsync(TradeType.Sell, _symbol.Name, volume, clickPrice, "ScalpingPanel - sell stop", stopLossPips, takeProfitPips);
                    }
                    if (ask > clickPrice)
                    {
                        // sell limit
                        _robot.PlaceStopOrderAsync(TradeType.Sell, _symbol.Name, volume, clickPrice, "ScalpingPanel - sell limit", stopLossPips, takeProfitPips);
                    }

                    pendingButton.Text = "P. SELL";
                    pendingTradeOpen = false;
                }
            }
        }



        private Panel CreateInputWithLabel(string label, string defaultValue, string inputKey)
        {
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = "0 0 0 0"
            };

            var textBlock = new TextBlock
            {
                Text = label
            };

            var input = new TextBox
            {
                Margin = "0 5 0 0",
                Text = defaultValue,
                Style = Styles.CreateInputStyle()
            };

            _inputMap.Add(inputKey, input);

            stackPanel.AddChild(textBlock);
            stackPanel.AddChild(input);

            return stackPanel;
        }


        private Panel CreateLabel(string label)
        {
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = "0 0 0 0"
            };

            var textBlock = new TextBlock
            {
                Text = label,
            };

            stackPanel.AddChild(textBlock);

            return stackPanel;
        }

        private Panel CreateLabeTodayTotal(string label)
        {
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = "0 0 0 0"
            };

            var textBlock = new TextBlock
            {
                Text = label,
                FontWeight = FontWeight.Bold
            };

            stackPanel.AddChild(textBlock);
            todayTotal = textBlock;
            return stackPanel;
        }
        private Panel CreateLabeTodayTotalPercent(string label)
        {
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = "0 0 0 0"
            };

            var textBlock = new TextBlock
            {
                Text = label,
                FontWeight = FontWeight.Bold
            };

            stackPanel.AddChild(textBlock);
            todayTotalPercent = textBlock;
            return stackPanel;
        }


        private Panel CreateLabelActual(string label)
        {
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = "0 0 0 0"
            };

            var textBlock = new TextBlock
            {
                Text = label,
                FontWeight = FontWeight.Bold
            };

            stackPanel.AddChild(textBlock);
            todayActual = textBlock;
            return stackPanel;
        }


        private double GetValueFromInput(string inputKey, double defaultValue)
        {
            double value;

            return double.TryParse(_inputMap[inputKey].Text, out value) ? value : defaultValue;
        }

        private void CloseAllOpen()
        {
            foreach (var position in _robot.Positions)
            {
                if (position.SymbolName == _symbol.Name)
                {
                    _robot.ClosePositionAsync(position);
                }
            }
        }

        private void CloseAllPending()
        {
            foreach (var position in _robot.PendingOrders)
            {
                if (position.SymbolName == _symbol.Name)
                {
                    
                    _robot.CancelPendingOrderAsync(position);
                }
            }
        }
    }
}
