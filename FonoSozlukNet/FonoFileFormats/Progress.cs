namespace FonoFileFormats;

public class Progress
{   private int _step = 0;
    private int _total = 0;
    public int Step 
    {
        get { return this._step;}
        set 
        {
            this._step = value;
            OnStepIncremented();
        }
    }
    public int Total 
    {
        get { return this._total;}
        set 
        {
            this._total = value;
            OnStepIncremented();
        }
    }

    public delegate void ProgressEventHandler(object source, EventArgs e);

    public event ProgressEventHandler StepIncremented;

    protected virtual void OnStepIncremented()
    {
        if (StepIncremented != null)
        {
            StepIncremented(this, EventArgs.Empty);
        }
    }
}
