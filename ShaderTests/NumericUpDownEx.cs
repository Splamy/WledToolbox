using System.ComponentModel;

namespace ShaderTests;

[ToolboxItem(true), DesignerCategory("code")]
public class NumericUpDownEx : NumericUpDown
{
    private static readonly object Event_CurrentEditValueChanged = new();
    private decimal m_CurrentEditValue = 0M;

    public NumericUpDownEx() { }

    [Bindable(true), Browsable(false), EditorBrowsable(EditorBrowsableState.Always)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public virtual decimal CurrentEditValue
    {
        get => m_CurrentEditValue;
        internal set
        {
            if (value != m_CurrentEditValue)
            {
                m_CurrentEditValue = value;
                OnCurrentEditValueChanged(EventArgs.Empty);
            }
        }
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        if (decimal.TryParse(Text, out decimal value))
        {
            CurrentEditValue = value;
            OnValueChanged(e);
        }
    }

    public event EventHandler CurrentEditValueChanged
    {
        add
        {
            Events.AddHandler(Event_CurrentEditValueChanged, value);
        }
        remove
        {
            Events.RemoveHandler(Event_CurrentEditValueChanged, value);
        }
    }

    protected virtual void OnCurrentEditValueChanged(EventArgs e)
    {
        if (Events[Event_CurrentEditValueChanged] is EventHandler evth) evth(this, e);
    }
}