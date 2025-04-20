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
    private Progress<ProgressArgs>? progressInstance;
    private IProgress<ProgressArgs>? progress;

    public Visibility Visibility
    {
        get => visibilityProperty?.GetValue(this) as Visibility? ?? Visibility.Hidden;
        set
        {
            if (progress is not null)
            {
                progress.Report(new ProgressArgs { Visibility = value });
            }
            else
            {
                visibilityProperty?.SetValue(this, value);
            }
        }
    }

    public double Minimum
    {
        get => minimumProperty?.GetValue(this) as double? ?? 0;
        set
        {
            if (progress is not null)
            {
                progress.Report(new ProgressArgs { Minimum = value });
            }
            else
            {
                minimumProperty?.SetValue(this, value);
            }
        }
    }

    public double Maximum
    {
        get => maximumProperty?.GetValue(this) as double? ?? 0;
        set
        {
            if (progress is not null)
            {
                progress.Report(new ProgressArgs { Maximum = value });
            }
            else
            {
                maximumProperty?.SetValue(this, value);
            }
        }
    }

    public double Value
    {
        get => valueProperty?.GetValue(this) as double? ?? 0;
        set
        {
            if (progress is not null)
            {
                progress.Report(new ProgressArgs { Value = value });
            }
            else
            {
                valueProperty?.SetValue(this, value);
            }
        }
    }

    public string Text
    {
        get => textProperty?.GetValue(this) as string ?? string.Empty;
        set
        {
            if (progress is not null)
            {
                progress.Report(new ProgressArgs { Text = value });
            }
            else
            {
                textProperty?.SetValue(this, value);
            }
        }
    }

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

    public void Report(string? message)
    {
        progress?.Report(new ProgressArgs
        {
            Text = message
        });
    }

    public IDisposable CreateReporter()
    {
        progressInstance = new Progress<ProgressArgs>(args =>
        {
            if (args.Visibility.HasValue)
            {
                Visibility = args.Visibility.Value;
            }
            if (args.Value.HasValue)
            {
                Value = args.Value.Value;
            }
            if (args.Minimum.HasValue)
            {
                Minimum = args.Minimum.Value;
            }
            if (args.Maximum.HasValue)
            {
                Maximum = args.Maximum.Value;
            }
            if (args.Text != null)
            {
                Text = args.Text;
            }
        });
        progress = progressInstance;
        return new Disposable(() =>
        {
            progressInstance = null;
            progress = null;
        });
    }

    private static void SetProperty<T>(
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

    private class ProgressArgs
    {
        public Visibility? Visibility { get; set; }
        public double? Value { get; set; }
        public double? Minimum { get; set; }
        public double? Maximum { get; set; }
        public string? Text { get; set; }
    }
}