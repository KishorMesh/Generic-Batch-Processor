using System;

namespace API.Exceptions
{
    /// <summary>
    /// Exception claas to handle UnHandledException raised by client executer
    /// </summary>
    public class UnHandledException : Exception
    {
    }

    public class CoordinatorStoppedException : Exception
    {
    }
}
