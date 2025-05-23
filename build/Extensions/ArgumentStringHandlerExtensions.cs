using Nuke.Common.Tooling;

static class ArgumentStringHandlerExtensions
{
    public enum IsSecret
    {
        No,
        Yes,
    }
    
    public static ArgumentStringHandler Append(this ArgumentStringHandler handler, string paramater, object value = null, IsSecret? isSecret = null)
    {
        handler.AppendLiteral(paramater);
        if (value is not null)
        {
            handler.AppendFormatted(value, format: (isSecret == IsSecret.Yes ? "r" : null));
        }

        return handler;
    }
}
