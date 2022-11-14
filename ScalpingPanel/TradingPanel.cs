using cAlgo.API.Internals;
using cAlgo.API;
using System.Collections.Generic;

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
        private Button pendingSellButton;
        private Button pendingBuyButton;

        private bool IsKeyboardActive = false;

        public TextBlock todayTotal;
        public TextBlock todayTotalPercent;
        public TextBlock todayActual;

        public TextBox trailingPipsInput;
        public TextBox trailingDelayPipsInput;


        private const string LotsInputKey = "LotsKey";
        private const string TakeProfitInputKey = "TPKey";
        private const string StopLossInputKey = "SLKey";
        private const string BreakEvenInputKey = "BEKey";
        private const string BreakEventDelayInputKey = "BEDKey";

        private readonly IDictionary<string, TextBox> _inputMap = new Dictionary<string, TextBox>();
        private readonly Robot _robot;
        private readonly Symbol _symbol;

        private Tradingkey KeyMarketBuy;
        private Tradingkey KeyMarketSell;
        private Tradingkey KeyMarketClose;
        private Tradingkey KeyPendingCancel;


        public TradingPanel(
            Robot robot,
            Symbol symbol,
            double defaultLots,
            double defaultStopLossPips,
            double defaultTakeProfitPips,
            bool keyboardActive,
            double trailingPips,
            double trailingDelayPips,
            Tradingkey keyMarketBuy,
            Tradingkey keyMarketSell,
            Tradingkey keyMarketClose,
            Tradingkey keyPendingCancel
            )
        {
            _robot = robot;
            _symbol = symbol;
            AddChild(CreateTradingPanel(defaultLots, defaultStopLossPips, defaultTakeProfitPips, trailingPips, trailingDelayPips));
            _robot.Chart.MouseDown += Chart_MouseDown;
            _robot.Chart.KeyDown += Chart_KeyDown;
            IsKeyboardActive = keyboardActive;

            KeyMarketBuy = keyMarketBuy;
            KeyMarketSell = keyMarketSell;
            KeyMarketClose = keyMarketClose;
            KeyPendingCancel = keyPendingCancel;
        }

        private void Chart_KeyDown(ChartKeyboardEventArgs obj)
        {
            if (this.IsKeyboardActive)
            {

                Tradingkey pressedKey = GetKeyAssigment(obj.Key);

                if (pressedKey == KeyMarketBuy)
                {
                    MarketBuy();
                }

                if (pressedKey == KeyMarketClose)
                {
                    CloseAllOpen();
                }
                if (pressedKey == KeyMarketSell)
                {
                    MarketSell();
                }
                if (pressedKey == KeyPendingCancel)
                {
                    CloseAllPending();
                }

            }
        }

        private ControlBase CreateTradingPanel(double defaultLots, double defaultStopLossPips, double defaultTakeProfitPips, double trailingPips, double trailingDelayPips)
        {
            var mainPanel = new StackPanel();

            var header = CreateHeader();
            mainPanel.AddChild(header);

            var contentPanel = CreateContentPanel(defaultLots, defaultStopLossPips, defaultTakeProfitPips, trailingPips, trailingDelayPips);
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
                Text = "Trading panel",
                Margin = "10 7",
                Style = Styles.CreateHeaderStyle()
            };

            headerBorder.Child = header;
            return headerBorder;
        }

        private StackPanel CreateContentPanel(double defaultLots, double defaultStopLossPips, double defaultTakeProfitPips, double trailingPips, double trailingDelayPips)
        {
            var contentPanel = new StackPanel
            {
                Margin = 10
            };
            var grid = new Grid(25, 3);
            grid.Columns[1].SetWidthInPixels(5);

            var sellButton = CreateButton("M. SELL", Styles.CreateSellButtonStyle(), actionSellMarket);
            grid.AddChild(sellButton, 0, 0);


            var buyButton = CreateButton("M. BUY", Styles.CreateBuyButtonStyle(), actionBuyMarket);
            grid.AddChild(buyButton, 0, 2);
            grid.Rows[1].SetHeightInPixels(5);

            var pendingSellButton = CreateButton("P. SELL", Styles.CreateSellButtonStyle(), actionSellPending);
            this.pendingSellButton = pendingSellButton;
            grid.AddChild(pendingSellButton, 2, 0);


            var pendingBuyButton = CreateButton("P. BUY", Styles.CreateBuyButtonStyle(), actionBuyPending);
            this.pendingBuyButton = pendingBuyButton;
            grid.AddChild(pendingBuyButton, 2, 2);
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

            var breakEvenInput = CreateTrailingWithLabel("Trailing stop", trailingPips.ToString("F1"), BreakEvenInputKey);
            grid.AddChild(breakEvenInput, 12, 0);

            var breakEventDelayInput = CreateTrailingDelayWithLabel("Odložený trail.", trailingDelayPips.ToString("F1"), BreakEventDelayInputKey);
            grid.AddChild(breakEventDelayInput, 12, 2);
            grid.Rows[13].SetHeightInPixels(8);

            var totalOpenLabel = CreateLabel("Zisk dnes");
            grid.AddChild(totalOpenLabel, 14, 0);

            var totalOpenPercentLabel = CreateLabel("Zisk dnes %");
            grid.AddChild(totalOpenPercentLabel, 14, 2);
            grid.Rows[15].SetHeightInPixels(5);

            var totalOpen = CreateLabeTodayTotal("0");
            grid.AddChild(totalOpen, 16, 0);

            var totalOpenPercent = CreateLabeTodayTotalPercent("0");
            grid.AddChild(totalOpenPercent, 16, 2);
            grid.Rows[17].SetHeightInPixels(5);

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
                if (this.pendingTradeOpen)
                {
                    this.pendingTradeType = -1;
                    this.pendingTradeOpen = false;
                    this.pendingBuyButton.Text = "P.BUY";
                    this.pendingSellButton.Text = "P.SELL";
                }
                else
                {
                    this.pendingTradeType = actionType;
                    this.pendingTradeOpen = true;
                    this.pendingBuyButton.Text = "Umístit ...";

                }
                pendingButton = button;
            }

            if (actionType == actionSellPending)
            {
                if (pendingTradeOpen)
                {
                    this.pendingTradeType = -1;
                    this.pendingTradeOpen = false;
                    this.pendingSellButton.Text = "P.SELL";
                    this.pendingBuyButton.Text = "P.BUY";
                }
                else
                {
                    this.pendingTradeType = actionType;
                    this.pendingTradeOpen = true;
                    this.pendingSellButton.Text = "Umístit ...";
                }
                pendingButton = button;
            }

            if (actionType == actionCloseOpen)
            {
                this.CloseAllOpen();
            }

            if (actionType == actionClosePending)
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
            if (pendingTradeOpen)
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

        private Panel CreateTrailingWithLabel(string label, string defaultValue, string inputKey)
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
            this.trailingPipsInput = input;
            stackPanel.AddChild(textBlock);
            stackPanel.AddChild(input);

            return stackPanel;
        }

        private Panel CreateTrailingDelayWithLabel(string label, string defaultValue, string inputKey)
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
            this.trailingDelayPipsInput = input;
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

            return double.TryParse(_inputMap[inputKey].Text.Replace(".", ","), out value) ? value : defaultValue;
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

        private Tradingkey GetKeyAssigment(Key k)
        {
            Tradingkey key = Tradingkey.NONE;
            if (k == Key.A)
                key = Tradingkey.a;
            if (k == Key.B)
                key = Tradingkey.b;
            if (k == Key.C)
                key = Tradingkey.c;
            if (k == Key.D)
                key = Tradingkey.d;
            if (k == Key.E)
                key = Tradingkey.e;
            if (k == Key.F)
                key = Tradingkey.f;
            if (k == Key.G)
                key = Tradingkey.g;
            if (k == Key.H)
                key = Tradingkey.h;
            if (k == Key.I)
                key = Tradingkey.i;
            if (k == Key.J)
                key = Tradingkey.j;
            if (k == Key.K)
                key = Tradingkey.k;
            if (k == Key.L)
                key = Tradingkey.l;
            if (k == Key.M)
                key = Tradingkey.m;
            if (k == Key.N)
                key = Tradingkey.n;
            if (k == Key.O)
                key = Tradingkey.o;
            if (k == Key.P)
                key = Tradingkey.p;
            if (k == Key.Q)
                key = Tradingkey.q;
            if (k == Key.R)
                key = Tradingkey.r;
            if (k == Key.S)
                key = Tradingkey.s;
            if (k == Key.T)
                key = Tradingkey.t;
            if (k == Key.U)
                key = Tradingkey.u;
            if (k == Key.V)
                key = Tradingkey.v;
            if (k == Key.W)
                key = Tradingkey.w;
            if (k == Key.X)
                key = Tradingkey.x;
            if (k == Key.Y)
                key = Tradingkey.y;
            if (k == Key.Z)
                key = Tradingkey.z;


            if (k == Key.NumPad0)
                key = Tradingkey.num_0;
            if (k == Key.NumPad1)
                key = Tradingkey.num_1;
            if (k == Key.NumPad2)
                key = Tradingkey.num_2;
            if (k == Key.NumPad3)
                key = Tradingkey.num_3;
            if (k == Key.NumPad4)
                key = Tradingkey.num_4;
            if (k == Key.NumPad5)
                key = Tradingkey.num_5;
            if (k == Key.NumPad6)
                key = Tradingkey.num_6;
            if (k == Key.NumPad7)
                key = Tradingkey.num_7;
            if (k == Key.NumPad8)
                key = Tradingkey.num_8;
            if (k == Key.NumPad9)
                key = Tradingkey.num_9;

            if (k == Key.Decimal)
                key = Tradingkey.num_decimal;

            return key;
        }
    }
}
