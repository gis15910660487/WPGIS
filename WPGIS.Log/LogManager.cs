
namespace WPGIS.Log
{
    ///备注:日志
    /// 日志级别    
    public enum Loglevel
    {
        /// <summary>
        /// 调试用输出信息
        /// </summary>
        Info = 0,
        /// <summary>
        /// SQL语句
        /// </summary>
        SQL = 1,
        /// <summary>
        /// 异常或错误信息
        /// </summary>
        Error = 2
    }

    /// <summary>
    /// 日志记录
    /// </summary>
    public class LogManager
    {
        /// <summary>
        /// 记录日志 默认为LogLevel=Info 级别
        /// </summary>
        /// <param name="log">日志内容</param>
        public static void AddLog(string log)
        {
            string logContent = (0) + "&" + log;
            WindowMessageHelper.SendMessage("LogMonitor", logContent);            
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="log">日志内容</param>
        public static void AddLog(Loglevel level, string log)
        {
            string logContent = ((int)level) + "&" + log;
            WindowMessageHelper.SendMessage("LogMonitor", logContent);            
        }
    }
}
