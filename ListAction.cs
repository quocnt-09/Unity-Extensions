using System;
using System.Collections.Generic;
using UnityEngine;

namespace QNT.Extension
{
    public class ListAction
    {
        private List<QAction> _list;
        private QAction _currentAction;
        private int timesRetry;
        private bool _isExecute;

        public int Counter { get; private set; }

        private float _timeDelay;
        private const float Delay = 0.2f;

        public ListAction()
        {
            _list = new List<QAction>();
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
        public void AddList(Delegate method, object[] Params = null, Action doneCallback = null)
        {
            _list.Add(new QAction {m_Action = method, m_Params = Params, m_DoneCallback = doneCallback});
            Counter = _list.Count;
            _timeDelay = Delay;
            Debug.Log($"<qnt> <--List--> Add: {method.Method.Name}({ToString(Params)})");
        }

        public void Execute()
        {
            _timeDelay = Delay;
            _isExecute = true;
            timesRetry = 2;
            _currentAction = _list[0];
            Debug.Log($"<qnt> <--List--> Execute: {_currentAction.m_Action.Method.Name}({ToString(_currentAction.m_Params)})");
            _currentAction.m_Action.DynamicInvoke(_currentAction.m_Params);
        }

        public void DoneCurrentAction(string method = "")
        {
            if (!_isExecute) return;
            if (!string.IsNullOrEmpty(method) && !_currentAction.m_Action.Method.Name.Equals(method)) return;

            _isExecute = false;
            _timeDelay = Delay;
            if (Counter > 0)
            {
                _list.RemoveAt(0);
            }
            Counter = _list.Count;

            if (_currentAction.IsNull()) return;
            
            Debug.Log($"<--List--> Done: {_currentAction.m_Action.Method.Name}({ToString(_currentAction.m_Params)})");
            _currentAction.m_DoneCallback?.Invoke();
        }

        /// <summary>
        /// return false when failed
        /// </summary>
        /// <returns></returns>
        public bool RetryAction()
        {
            if (timesRetry > 0)
            {
                _currentAction.m_Action.DynamicInvoke(_currentAction.m_Params);
                _isExecute = true;
                timesRetry--;
                Debug.Log($"<--List--> Retry: {_currentAction.m_Action.Method.Name}");
                return true;
            }

            return false;
        }

        public bool NextAction(float timeUpdate, bool executeNext = false)
        {
            _timeDelay -= timeUpdate;
            if (Counter <= 0 || _isExecute || _timeDelay > 0) return false;

            if (executeNext)
            {
                Execute();
            }

            return true;
        }

        public int CountMethod(string methodName)
        {
            var count = 0;
            foreach (var action in _list)
            {
                if (action.m_Action.Method.Name.Equals(methodName)) count++;
            }

            Debug.Log($"<--List--> Count: {methodName}: {count}");
            return count;
        }

        public void ClearAll()
        {
            _list.Clear();
            Counter = 0;
            _isExecute = false;
        }
    }
}