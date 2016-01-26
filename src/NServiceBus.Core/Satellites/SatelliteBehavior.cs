namespace NServiceBus.Pipeline
{
    /// <summary>
    /// A base class for satellite behaviors.
    /// </summary>
    public abstract class SatelliteBehavior : PipelineTerminator<IIncomingPhysicalMessageContext>
    {
    }
}