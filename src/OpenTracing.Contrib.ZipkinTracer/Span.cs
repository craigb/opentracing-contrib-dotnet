using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenTracing.Contrib.ZipkinTracer
{
    public class Span : ISpan
    {
        private readonly ZipkinTracer _tracer;
        private readonly HighResDuration _duration;

        private List<Annotation> _annotations;
        private List<BinaryAnnotation> _binaryAnnotations;

        public ISpanContext Context => TypedContext;
        public SpanContext TypedContext { get; }

        public string OperationName { get; private set; }

        public DateTime StartTimestamp => _duration.StartTimestamp;
        public DateTime? FinishTimestamp => _duration.FinishTimestamp;

        public IEnumerable<Annotation> Annotations => _annotations ?? Enumerable.Empty<Annotation>();
        public IEnumerable<BinaryAnnotation> BinaryAnnotations => _binaryAnnotations ?? Enumerable.Empty<BinaryAnnotation>();

        public Span(
            ZipkinTracer tracer,
            SpanContext context,
            string operationName,
            DateTime? startTimestamp,
            List<KeyValuePair<string, object>> tags)
        {
            if (tracer == null)
                throw new ArgumentNullException(nameof(tracer));

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (string.IsNullOrWhiteSpace(operationName))
                throw new ArgumentNullException(nameof(operationName));

            _tracer = tracer;
            _duration = new HighResDuration(context.Clock, startTimestamp);

            TypedContext = context;
            OperationName = operationName;

            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    AddTag(tag.Key, tag.Value);
                }
            }
        }

        public ISpan SetOperationName(string operationName)
        {
            if (string.IsNullOrWhiteSpace(operationName))
                throw new ArgumentNullException(nameof(operationName));

            OperationName = operationName;
            return this;
        }

        public ISpan SetTag(string key, string value)
        {
            return AddTag(key, value);
        }

        public ISpan SetTag(string key, double value)
        {
            return AddTag(key, value);
        }

        public ISpan SetTag(string key, int value)
        {
            return AddTag(key, value);
        }

        public ISpan SetTag(string key, bool value)
        {
            return AddTag(key, value);
        }

        public ISpan Log(IEnumerable<KeyValuePair<string, object>> fields)
        {
            return Log(_duration.GetUtcNow(), fields);
        }

        public ISpan Log(DateTime timestamp, IEnumerable<KeyValuePair<string, object>> fields)
        {
            if (fields == null)
                return this;

            // TODO @cweiss How should we store fields?
            string value = string.Join(", ", fields.Select(x => $"{x.Key}:{x.Value}"));
            return AddAnnotation(timestamp, value);
        }

        public ISpan Log(string eventName)
        {
            return AddAnnotation(_duration.GetUtcNow(), eventName);
        }

        public ISpan Log(DateTime timestamp, string eventName)
        {
            return AddAnnotation(timestamp, eventName);
        }

        public ISpan SetBaggageItem(string key, string value)
        {
            TypedContext.SetBaggageItem(key, value);
            return this;
        }

        public string GetBaggageItem(string key)
        {
            return TypedContext.GetBaggageItem(key);
        }

        public void Finish()
        {
            FinishInternal(null);
        }

        public void Finish(DateTime finishTimestamp)
        {
            FinishInternal(finishTimestamp);
        }

        public void Dispose()
        {
            Finish();
        }

        private void FinishInternal(DateTime? finishTimestamp)
        {
            if (FinishTimestamp.HasValue)
                return;

            _duration.Finish(finishTimestamp);

            _tracer.ReportSpan(this);
        }

        private ISpan AddAnnotation(DateTime timestamp, string eventName)
        {
            if (string.IsNullOrWhiteSpace(eventName))
                throw new ArgumentNullException(nameof(eventName));

            if (_annotations == null)
                _annotations = new List<Annotation>();

            _annotations.Add(new Annotation(_tracer.Endpoint, eventName, timestamp));
            return this;
        }

        private ISpan AddTag(string key, object value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            bool added = AddTagAsAnnotation(key, value);
            if (!added)
            {
                AddTagAsBinaryAnnotation(key, value);
            }

            return this;
        }

        private bool AddTagAsAnnotation(string key, object value)
        {
            string annotationValue = null;

            string stringValue = value?.ToString();

            if (key == Tags.SpanKind && stringValue == Tags.SpanKindServer)
            {
                annotationValue = AnnotationConstants.ServerReceive;
            }
            else if (key == Tags.SpanKind && stringValue == Tags.SpanKindClient)
            {
                annotationValue = AnnotationConstants.ClientSend;
            }

            if (annotationValue != null)
            {
                if (_annotations == null)
                    _annotations = new List<Annotation>();

                _annotations.Add(new Annotation(_tracer.Endpoint, annotationValue, StartTimestamp));
                return true;
            }

            return false;
        }

        private void AddTagAsBinaryAnnotation(string key, object value)
        {
            if (_binaryAnnotations == null)
                _binaryAnnotations = new List<BinaryAnnotation>();

            if (key == Tags.Component)
                key = AnnotationConstants.LocalComponent;

            _binaryAnnotations.Add(new BinaryAnnotation
            {
                Host = _tracer.Endpoint,
                Key = key,
                Value = value
            });
        }
    }
}