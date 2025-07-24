using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TradingPlatform.BusinessLayer;
using TradingPlatform.BusinessLayer.Chart;

public class HTFCandles : Indicator
{

    private Period HTFPerfiod = new Period(BasePeriod.Hour, 1);

    private HistoricalData hdm;
    private Brush bullishBrush;
    private Brush bearishBrush;

    private Color bullishColor = ColorTranslator.FromHtml("#1f865e");
    private Color bearishColor = ColorTranslator.FromHtml("#b25432");

    private bool showFiboLevels = false;
    private bool showAllCandlesFiboLevels = false;
    private Color fiboLevelsColor = Color.LightBlue;

    private Color rangeFibosLevelsColor = Color.Violet;

    private int candlesCount = 1;
    private int indicator_offset = 100;
    private int candles_width = 20;


    public override IList<SettingItem> Settings
    {
        get
        {
            IList<SettingItem> settings = base.Settings;

            settings.Add(new SettingItemPeriod("HTFPerfiod", HTFPerfiod)
            {
                Text = "HTF Perfiod",
                SortIndex = 0,
            });

            settings.Add(new SettingItemColor("bullishColor", bullishColor)
            {
                Text = "Bullish Candle Color",
                SortIndex = 1,
            });

            settings.Add(new SettingItemColor("bearishColor", bearishColor)
            {
                Text = "Bearish Candle Color",
                SortIndex = 2,
            });

            settings.Add(new SettingItemInteger("candlesCount", candlesCount)
            {
                Text = "Candles Count",
                SortIndex = 3,
                Maximum = 100,
                Minimum = 0
            });

            settings.Add(new SettingItemBoolean("showFiboLevels", showFiboLevels)
            {
                Text = "Show Fibo Levels",
                SortIndex = 4,
            });

            settings.Add(new SettingItemColor("fiboLevelsColor", fiboLevelsColor)
            {
                Text = "Fibo Levels Color",
                SortIndex = 5,
            });

            settings.Add(new SettingItemBoolean("showAllCandlesFiboLevels", showAllCandlesFiboLevels)
            {
                Text = "Show All Candles Fibo Levels",
                SortIndex = 6,
            });

            settings.Add(new SettingItemColor("rangeFibosLevelsColor", rangeFibosLevelsColor)
            {
                Text = "Range Fibos Levels Color",
                SortIndex = 7,
            });

            settings.Add(new SettingItemInteger("candles_width", candles_width)
            {
                Text = "Candles Width",
                SortIndex = 9,
            });

            settings.Add(new SettingItemInteger("indicator_offset", indicator_offset)
            {
                Text = "X Axis Offset",
                SortIndex = 10,
            });

            return settings;
        }
        set
        {
            if (value.TryGetValue<Period>("HTFPerfiod", out var value1)) HTFPerfiod = value1;

            if (value.TryGetValue<Color>("bullishColor", out var value2)) bullishColor = value2;
            if (value.TryGetValue<Color>("bearishColor", out var value3)) bearishColor = value3;
            if (value.TryGetValue<int>("candlesCount", out var value4)) candlesCount = value4;
            if (value.TryGetValue<int>("indicator_offset", out var value5)) indicator_offset = value5;
            if (value.TryGetValue<bool>("showFiboLevels", out var value6)) showFiboLevels = value6;
            if (value.TryGetValue<Color>("fiboLevelsColor", out var value7)) fiboLevelsColor = value7;
            if (value.TryGetValue<int>("candles_width", out var value8)) candles_width = value8;
            if (value.TryGetValue<bool>("showAllCandlesFiboLevels", out var value9)) showAllCandlesFiboLevels = value9;
            if (value.TryGetValue<Color>("rangeFibosLevelsColor", out var value10)) rangeFibosLevelsColor = value10;


            OnSettingsUpdated();
        }
    }

    public HTFCandles()
        : base()
    {
        Name = "HTF Candles";
        Description = "Shows candles from another Timeframe";

        SeparateWindow = false;
    }

    /// <summary>
    /// This function will be called after creating an indicator as well as after its input params reset or chart (symbol or timeframe) updates.
    /// </summary>
    protected override void OnInit()
    {
        this.hdm = this.Symbol.GetHistory(HTFPerfiod, Symbol.HistoryType, 100);
    }

    /// <summary>
    /// Calculation entry point. This function is called when a price data updates. 
    /// Will be runing under the HistoricalBar mode during history loading. 
    /// Under NewTick during realtime. 
    /// Under NewBar if start of the new bar is required.
    /// </summary>
    /// <param name="args">Provides data of updating reason and incoming price.</param>
    protected override void OnUpdate(UpdateArgs args)
    {
    }

    public override void OnPaintChart(PaintChartEventArgs args)
    {
        int offset = indicator_offset;
        for(int i = candlesCount - 1; i >= 0; i--)
        {
            PaintCandle(args, i, offset);
            offset += candles_width + 2;

            if (showAllCandlesFiboLevels && i == 0) PaintFibosRange(args, offset);
        }
    }

    private void PaintCandle(PaintChartEventArgs args, int candleIndex = 0, int offset = 100)
    {
        IChartWindow chartWindow = base.CurrentChart.Windows[args.WindowIndex];
        var font = new Font("Arial", 9);

        var candle_mid = candles_width / 2;


        var candle = GetBar(candleIndex);

        var coordYOpen = (float)chartWindow.CoordinatesConverter.GetChartY((candle.Open));
        var coordYClose = (float)chartWindow.CoordinatesConverter.GetChartY((candle.Close));
        var coordYHigh = (float)chartWindow.CoordinatesConverter.GetChartY((candle.High));
        var coordYLow = (float)chartWindow.CoordinatesConverter.GetChartY((candle.Low));


        RectangleF clipBounds = args.Graphics.ClipBounds;

        var candleTime = GetBar().TimeRight;


        var candleX = (float)chartWindow.CoordinatesConverter.GetChartX(candleTime) + offset;



        var point1 = new PointF(candleX, coordYOpen);
        var point2 = new PointF(candleX + candles_width, coordYOpen);
        var point3 = new PointF(candleX, coordYClose);
        var point4 = new PointF(candleX + candles_width, coordYClose);


        SolidBrush brush = new SolidBrush(candle.Open < candle.Close ? bullishColor : bearishColor);

        // Candle body
        args.Graphics.FillPolygon(brush, point1, point2, point4, point3);

        // High and low
        args.Graphics.FillPolygon(brush, new PointF(candleX + candle_mid, coordYHigh), new PointF(candleX + candle_mid, coordYLow), new PointF(candleX + candle_mid + 1, coordYLow), new PointF(candleX + candle_mid + 1, coordYHigh));

        if (showFiboLevels)
        {
            var pen = new Pen(fiboLevelsColor);
            var fibo618 = (candle.High - candle.Low) * 0.618 + candle.Low;
            var fibo50 = (candle.High - candle.Low) * 0.5 + candle.Low;
            var fibo382 = (candle.High - candle.Low) * 0.382 + candle.Low;


            var fibo618CoordY = (float)chartWindow.CoordinatesConverter.GetChartY(fibo618);
            args.Graphics.DrawLine(pen, new PointF(candleX + candle_mid, fibo618CoordY), new PointF(candleX+candles_width, fibo618CoordY));

            var fibo50CoordY = (float)chartWindow.CoordinatesConverter.GetChartY(fibo50);
            args.Graphics.DrawLine(pen, new PointF(candleX + candle_mid, fibo50CoordY), new PointF(candleX+candles_width, fibo50CoordY));

            var fibo382CoordY = (float)chartWindow.CoordinatesConverter.GetChartY(fibo382);
            args.Graphics.DrawLine(pen, new PointF(candleX + candle_mid, fibo382CoordY), new PointF(candleX+candles_width, fibo382CoordY));

            
        }

    }

    private void PaintFibosRange(PaintChartEventArgs args, int offset)
    {
        IChartWindow chartWindow = base.CurrentChart.Windows[args.WindowIndex];

        var candles = this.hdm.Reverse().Take(candlesCount);
        double max = candles.Select(c => ((HistoryItemBar)c).High).Max();
        double min = candles.Select(c => ((HistoryItemBar)c).Low).Min();

        var fibo618 = (max - min) * 0.618 + min;
        var fibo50 = (max - min) * 0.5 + min;
        var fibo382 = (max - min) * 0.382 + min;


        var candleTime = GetBar().TimeRight;
        var candleX = (float)chartWindow.CoordinatesConverter.GetChartX(candleTime) + offset;

        var pen = new Pen(rangeFibosLevelsColor);

        var fibo618CoordY = (float)chartWindow.CoordinatesConverter.GetChartY(fibo618);
        args.Graphics.DrawLine(pen, new PointF(candleX + candles_width / 2, fibo618CoordY), new PointF(candleX + candles_width, fibo618CoordY));

        var fibo50CoordY = (float)chartWindow.CoordinatesConverter.GetChartY(fibo50);
        args.Graphics.DrawLine(pen, new PointF(candleX + candles_width / 2, fibo50CoordY), new PointF(candleX + candles_width, fibo50CoordY));

        var fibo382CoordY = (float)chartWindow.CoordinatesConverter.GetChartY(fibo382);
        args.Graphics.DrawLine(pen, new PointF(candleX + candles_width / 2, fibo382CoordY), new PointF(candleX + candles_width, fibo382CoordY));
    }

    private HistoryItemBar GetBar(int index = 0) => ((HistoryItemBar)this.hdm[index]);


}
