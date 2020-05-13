using System;
using System.Collections.Generic;
using UnityEngine;

namespace QNT.Extension
{
    public struct QAction
    {
        public Delegate m_Action;
        public object[] m_Params;
        public Action m_DoneCallback;
    }

    public class QueueAction
    {
        private Queue<QAction> _queue;
        private QAction _currentAction;

        private bool _isExecute;
        private float _timeDelay;
        private const float Delay = 0.2f;

        public int Counter { get; private set; }

        public QueueAction()
        {
            _queue = new Queue<QAction>();
            Counter = 0;
            _isExecute = false;
            _timeDelay = 0;
        }

        private string ToString(object[] Params)
        {
            var txt = "";
            if (Params != null && Params.Length > 0)
            {
                for (int i = 0; i < Params.Length; i++)
                {
                    if (i > 0)
                    {
                        txt += ",";
                    }

                    txt += Params[i].ToString();
                }
            }

            return txt;
        }

        public void AddQueue(Delegate method, object[] Params = null, Action doneCallback = null)
        {
            _queue.Enqueue(new QAction { m_Action = method, m_Params = Params, m_DoneCallback = doneCallback });
            Counter = _queue.Count;
            _timeDelay = Delay;
            Debug.Log($"<--Queue--> Add: {method.Method.Name}({ToString(Params)})");
        }

        public void Execute()
        {
            _isExecute = true;
            _timeDelay = Delay;

            _currentAction = _queue.Dequeue();
            Counter = _queue.Count;
            Debug.Log($"<--Queue--> Execute: {_currentAction.m_Action.Method.Name}({ToString(_currentAction.m_Params)})");
            _currentAction.m_Action.DynamicInvoke(_currentAction.m_Params);
        }

        public void DoneCurrentAction(string method = "")
        {
            if (!_isExecute) return;
            if (!string.IsNullOrEmpty(method) && !_currentAction.m_Action.Method.Name.Equals(method)) return;

            _isExecute = false;
            _timeDelay = Delay;
            if (_currentAction.IsNull()) return;
            Debug.Log($"<--Queue--> Done: {_currentAction.m_Action.Method.Name}({ToString(_currentAction.m_Params)})");
            _currentAction.m_DoneCallback?.Invoke();
        }
        public void Done(bool nextAction)
        {
            _isExecute = false;
            if (nextAction)
            {
                NextAction(10, nextAction);
            }
        }
        public bool NextAction(float timeUpdate, bool executeNext = false)
        {
            _timeDelay -= timeUpdate;
            if (Counter <= 0 || _isExecute || _timeDelay > 0) return false;

            if (executeNext)
            {
                Execute();
                return false;
            }

            return true;
        }

        public int CountMethod(string methodName)
        {
            var count = 0;
            foreach (var action in _queue)
            {
                if (action.m_Action.Method.Name.Equals(methodName)) count++;
            }

            Debug.Log($"<--Queue--> Count: {methodName}: {count}");
            return count;
        }
    }
}