using SharpDX.Direct2D1;
using System.Linq.Expressions;

namespace ShaderTests.EffectControls;

internal class EffectEditor<T> : Control where T : Effect
{
    protected ActiveEffect ActiveEffect { get; }
    protected T Effect => (T)ActiveEffect.Effect;
    protected Dictionary<string, object> Model => ActiveEffect.Model;

    protected void SetModel<K>(Expression<Func<T, K>> prop, K value)
    {
        var member = (MemberExpression)prop.Body;
        var propInfo = (System.Reflection.PropertyInfo)member.Member;
        Model[propInfo.Name] = value;

        ActiveEffect?.ApplyModel();
    }

    protected K GetModel<K>(Expression<Func<T, K>> prop)
    {
        var member = (MemberExpression)prop.Body;
        var propInfo = (System.Reflection.PropertyInfo)member.Member;

        if (Model.TryGetValue(propInfo.Name, out var value))
        {
            return (K)value;
        }
        else
        {
            // return current value
            return (K)propInfo.GetValue(Effect);
        }
    }

    protected void AutoLayout()
    {
        foreach (var control in Controls.OfType<Control>())
        {
            control.Dock = DockStyle.Top;
            control.BringToFront();
        }
    }

    protected TControl C<TControl>(TControl c) where TControl : Control
    {
        Controls.Add(c);
        c.Dock = DockStyle.Top;
        c.BringToFront();
        return c;
    }

    protected void C<TControl>(TControl c, out TControl cOut) where TControl : Control
    {
        cOut = C(c);
    }

    public EffectEditor(ActiveEffect ae)
    {
        Dock = DockStyle.Fill;
        ActiveEffect = ae;
    }
}