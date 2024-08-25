using System.ComponentModel;

namespace MyTelegramBot.Enums
{
    public enum Duration
    {
        [Description("неделя")]
        Week = 0,
        [Description("2 недели")]
        TwoWeek = 1,
        [Description("месяц")]
        Month = 2,
        [Description("3 месяца")]
        ThreeMonth = 3,
        [Description("полгода")]
        SixMonth = 4,
        [Description("год")]
        Year = 5
    }
}
