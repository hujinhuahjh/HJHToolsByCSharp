namespace HJHTools.Entity.Enums;

public enum CmdExecuteResultEnum
{
    Init,
        
    /// <summary>
    /// 命令执行成功
    /// </summary>
    Success,

    /// <summary>
    /// 命令执行需要重发
    /// </summary>
    Retry,

    /// <summary>
    /// 命令执行失败
    /// </summary>
    Failure,
        
    /// <summary>
    /// 命令执行超时
    /// </summary>
    Timeout,
}