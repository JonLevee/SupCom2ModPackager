using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;

namespace SupCom2ModPackager;

public class ProgressReporter
{
    private PropertyInfo? visibilityProperty;
    private PropertyInfo? minimumProperty;
    private PropertyInfo? maximumProperty;
    private PropertyInfo? valueProperty;
    private PropertyInfo? textProperty;

    public void SetVisibilityProperty(Expression<Func<Visibility>> visibilityExpression)
    {
        SetProperty(visibilityExpression, ref visibilityProperty);
    }

    public void SetMinimumProperty(Expression<Func<double>> minimumExpression)
    {
        SetProperty(minimumExpression, ref minimumProperty);
    }

    public void SetMaximumProperty(Expression<Func<double>> maximumExpression)
    {
        SetProperty(maximumExpression, ref maximumProperty);
    }

    public void SetValueProperty(Expression<Func<double>> valueExpression)
    {
        SetProperty(valueExpression, ref valueProperty);
    }

    public void SetTextProperty(Expression<Func<string>> textExpression)
    {
        SetProperty(textExpression, ref textProperty);
    }

    public void Report(int? min, int? max, string? message)
    {
    }

    private void SetProperty<T>(
        Expression<Func<T>> expression,
        ref PropertyInfo? property,
        [CallerMemberName] string? callerName = null)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            if (memberExpression.Member is PropertyInfo)
            {
                property = memberExpression.Member as PropertyInfo;
                return;
            }
        }
        throw new ArgumentException("Expression must represent a property.", callerName);
    }
}