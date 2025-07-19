using System.Collections.Generic;
using System.Drawing;
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

    private int candlesCount = 1;
    private int indicator_offset = 100;


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

            settings.Add(new SettingItemInteger("indicator_offset", indicator_offset)
            {
                Text = "X Axis Offset",
                SortIndex = 10,
                Minimum = 0
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


            OnSettingsUpdated();
        }
    }

    public HTFCandles()
        : base()
    {
        // Defines indicator's name and description.
        Name = "HTF Candles";
        Description = "";

        // Defines line on demand with particular parameters.
        //AddLineSeries("line1", Color.CadetBlue, 1, LineStyle.Solid);


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
        //IChartWindow chartWindow = base.CurrentChart.Windows[args.WindowIndex];

        int offset = indicator_offset;
        for(int i = candlesCount - 1; i >= 0; i--)
        {
            PaintCandle(args, i, offset);
            offset += 22;
        }

    }

    private void PaintCandle(PaintChartEventArgs args, int candleIndex = 0, int offset = 100)
    {
        IChartWindow chartWindow = base.CurrentChart.Windows[args.WindowIndex];
        var font = new Font("Arial", 9);


        var candle = GetBar(candleIndex);

        var coordYOpen = (float)chartWindow.CoordinatesConverter.GetChartY((candle.Open));
        var coordYClose = (float)chartWindow.CoordinatesConverter.GetChartY((candle.Close));
        var coordYHigh = (float)chartWindow.CoordinatesConverter.GetChartY((candle.High));
        var coordYLow = (float)chartWindow.CoordinatesConverter.GetChartY((candle.Low));


        RectangleF clipBounds = args.Graphics.ClipBounds;

        var candleTime = GetBar().TimeRight;


        var candleX = (float)chartWindow.CoordinatesConverter.GetChartX(candleTime) + offset;



        var point1 = new PointF(candleX, coordYOpen);
        var point2 = new PointF(candleX + 20, coordYOpen);
        var point3 = new PointF(candleX, coordYClose);
        var point4 = new PointF(candleX + 20, coordYClose);


        SolidBrush brush = new SolidBrush(candle.Open < candle.Close ? bullishColor : bearishColor);

        // Candle body
        args.Graphics.FillPolygon(brush, point1, point2, point4, point3);

        // High and low
        args.Graphics.FillPolygon(brush, new PointF(candleX + 10, coordYHigh), new PointF(candleX + 10, coordYLow), new PointF(candleX + 11, coordYLow), new PointF(candleX + 11, coordYHigh));


    }

    private HistoryItemBar GetBar(int index = 0) => ((HistoryItemBar)this.hdm[index]);


}
