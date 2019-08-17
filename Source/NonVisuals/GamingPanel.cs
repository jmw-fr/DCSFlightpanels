﻿using System;
using System.Collections.Generic;
using ClassLibraryCommon;
using DCS_BIOS;
using HidLibrary;

namespace NonVisuals
{

    public abstract class GamingPanel : IProfileHandlerListener, IDcsBiosDataListener
    {
        //These events can be raised by the descendants of this class.
        public delegate void SwitchesHasBeenChangedEventHandler(object sender, SwitchesChangedEventArgs e);
        public event SwitchesHasBeenChangedEventHandler OnSwitchesChangedA;

        public delegate void PanelDataToDcsAvailableEventHandler(object sender, PanelDataToDCSBIOSEventEventArgs e);
        public event PanelDataToDcsAvailableEventHandler OnPanelDataAvailableA;

        public delegate void DeviceAttachedEventHandler(object sender, PanelEventArgs e);
        public event DeviceAttachedEventHandler OnDeviceAttachedA;

        public delegate void DeviceDetachedEventHandler(object sender, PanelEventArgs e);
        public event DeviceDetachedEventHandler OnDeviceDetachedA;

        public delegate void SettingsHasChangedEventHandler(object sender, PanelEventArgs e);
        public event SettingsHasChangedEventHandler OnSettingsChangedA;

        public delegate void SettingsHasBeenAppliedEventHandler(object sender, PanelEventArgs e);
        public event SettingsHasBeenAppliedEventHandler OnSettingsAppliedA;

        public delegate void SettingsClearedEventHandler(object sender, PanelEventArgs e);
        public event SettingsClearedEventHandler OnSettingsClearedA;

        public delegate void LedLightChangedEventHandler(object sender, LedLightChangeEventArgs e);
        public event LedLightChangedEventHandler OnLedLightChangedA;

        public delegate void UpdatesHasBeenMissedEventHandler(object sender, DCSBIOSUpdatesMissedEventArgs e);
        public event UpdatesHasBeenMissedEventHandler OnUpdatesHasBeenMissed;

        //For those that wants to listen to this panel
        public void Attach(IGamingPanelListener iGamingPanelListener)
        {
            OnDeviceAttachedA += iGamingPanelListener.DeviceAttached;
            OnSwitchesChangedA += iGamingPanelListener.SwitchesChanged;
            OnPanelDataAvailableA += iGamingPanelListener.PanelDataAvailable;
            OnSettingsAppliedA += iGamingPanelListener.SettingsApplied;
            OnLedLightChangedA += iGamingPanelListener.LedLightChanged;
            OnSettingsClearedA += iGamingPanelListener.SettingsCleared;
            OnUpdatesHasBeenMissed += iGamingPanelListener.UpdatesHasBeenMissed;
            OnSettingsChangedA += iGamingPanelListener.PanelSettingsChanged;
        }

        //For those that wants to listen to this panel
        public void Detach(IGamingPanelListener iGamingPanelListener)
        {
            OnDeviceAttachedA -= iGamingPanelListener.DeviceAttached;
            OnSwitchesChangedA -= iGamingPanelListener.SwitchesChanged;
            OnPanelDataAvailableA -= iGamingPanelListener.PanelDataAvailable;
            OnSettingsAppliedA -= iGamingPanelListener.SettingsApplied;
            OnLedLightChangedA -= iGamingPanelListener.LedLightChanged;
            OnSettingsClearedA -= iGamingPanelListener.SettingsCleared;
            OnUpdatesHasBeenMissed -= iGamingPanelListener.UpdatesHasBeenMissed;
            OnSettingsChangedA -= iGamingPanelListener.PanelSettingsChanged;
        }

        //For those that wants to listen to this panel when it's settings change
        public void Attach(IProfileHandlerListener iProfileHandlerListener)
        {
            OnSettingsChangedA += iProfileHandlerListener.PanelSettingsChanged;
        }

        //For those that wants to listen to this panel
        public void Detach(IProfileHandlerListener iProfileHandlerListener)
        {
            OnSettingsChangedA -= iProfileHandlerListener.PanelSettingsChanged;
        }

        //Used by any but descendants that wants to see buttons that have changed, UI for example
        protected virtual void OnSwitchesChanged(HashSet<object> hashSet)
        {
            OnSwitchesChangedA?.Invoke(this, new SwitchesChangedEventArgs() { UniqueId = InstanceId, GamingPanelEnum = _typeOfGamingPanel, Switches = hashSet });
        }

        //Used by any but descendants that wants to see buttons that have changed, UI for example
        protected virtual void OnPanelDataAvailable(string stringData)
        {
            OnPanelDataAvailableA?.Invoke(this, new PanelDataToDCSBIOSEventEventArgs() { StringData = stringData });
        }


        protected virtual void OnDeviceAttached()
        {
            //IsAttached = true;
            OnDeviceAttachedA?.Invoke(this, new PanelEventArgs() { UniqueId = InstanceId, GamingPanelEnum = _typeOfGamingPanel });
        }


        protected virtual void OnDeviceDetached()
        {
            //IsAttached = false;
            OnDeviceDetachedA?.Invoke(this, new PanelEventArgs() { UniqueId = InstanceId, GamingPanelEnum = _typeOfGamingPanel });
        }


        protected virtual void OnSettingsChanged()
        {
            OnSettingsChangedA?.Invoke(this, new PanelEventArgs() { UniqueId = InstanceId, GamingPanelEnum = _typeOfGamingPanel });
        }


        protected virtual void OnSettingsApplied()
        {
            OnSettingsAppliedA?.Invoke(this, new PanelEventArgs() { UniqueId = InstanceId, GamingPanelEnum = _typeOfGamingPanel });
        }


        protected virtual void OnSettingsCleared()
        {
            OnSettingsClearedA?.Invoke(this, new PanelEventArgs() { UniqueId = InstanceId, GamingPanelEnum = _typeOfGamingPanel });
        }

        public void PanelSettingsChanged(object sender, PanelEventArgs e)
        {
            //do nada
        }

        public void PanelSettingsReadFromFile(object sender, SettingsReadFromFileEventArgs e)
        {
            ClearPanelSettings(this);
            ImportSettings(e.Settings);
        }

        public void ClearPanelSettings(object sender)
        {
            ClearSettings();
            OnSettingsCleared();
        }

        private int _vendorId;
        private int _productId;
        private Exception _lastException;
        private readonly object _exceptionLockObject = new object();
        private GamingPanelEnum _typeOfGamingPanel;
        private bool _isDirty;
        //private bool _isAttached;
        private bool _forwardPanelEvent;
        private static readonly object _lockObject = new object();
        private static readonly List<GamingPanel> GamingPanels = new List<GamingPanel>();
        private bool _settingsLoading = false;
        /*
         * IMPORTANT STUFF
         */
        private readonly DCSBIOSOutput _updateCounterDCSBIOSOutput;
        private static readonly object UpdateCounterLockObject = new object();
        private uint _count;
        private bool _synchedOnce;
        private readonly Guid _guid = Guid.NewGuid();
        private readonly string _hash;

        protected GamingPanel(GamingPanelEnum typeOfGamingPanel, HIDSkeleton hidSkeleton)
        {
            _typeOfGamingPanel = typeOfGamingPanel;
            HIDSkeletonBase = hidSkeleton;
            if (Common.IsOperationModeFlagSet(OperationFlag.DCSBIOSOutputEnabled))
            {
                _updateCounterDCSBIOSOutput = DCSBIOSOutput.GetUpdateCounter();
            }
            _hash = Common.GetMd5Hash(hidSkeleton.InstanceId);
        }

        public abstract string SettingsVersion();
        public abstract void Startup();
        public abstract void Shutdown();
        public abstract void ClearSettings();
        public abstract void ImportSettings(List<string> settings);
        public abstract List<string> ExportSettings();
        public abstract void SavePanelSettings(object sender, ProfileHandlerEventArgs e);
        public abstract void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e);
        //public abstract void DcsBiosDataReceived(byte[] array);
        //public abstract void GetDcsBiosData(byte[] bytes);
        protected HIDSkeleton HIDSkeletonBase;
        private bool _closed;
        public long ReportCounter = 0;
        
        protected bool FirstReportHasBeenRead = false;
        protected abstract void GamingPanelKnobChanged(IEnumerable<object> hashSet);

        protected abstract void StartListeningForPanelChanges();

        protected void UpdateCounter(uint address, uint data)
        {
            lock (UpdateCounterLockObject)
            {
                if (_updateCounterDCSBIOSOutput.Address == address)
                {
                    var newCount = _updateCounterDCSBIOSOutput.GetUIntValue(data);
                    if (!_synchedOnce)
                    {
                        _count = newCount;
                        _synchedOnce = true;
                        return;
                    }
                    //Max is 255
                    if ((newCount == 0 && _count == 255) || newCount - _count == 1)
                    {
                        //All is well
                        _count = newCount;
                    }
                    else if (newCount - _count != 1)
                    {
                        //Not good
                        if (OnUpdatesHasBeenMissed != null)
                        {
                            OnUpdatesHasBeenMissed(this, new DCSBIOSUpdatesMissedEventArgs() { UniqueId = HIDSkeletonBase.InstanceId, GamingPanelEnum = _typeOfGamingPanel, Count = (int)(newCount - _count) });
                            _count = newCount;
                        }
                    }
                }
            }
        }

        public void SelectedAirframe(object sender, AirframeEventArgs e)
        {

        }

        //User can choose not to in case switches needs to be reset but not affect the airframe. E.g. after crashing.
        public void SetForwardKeyPresses(object sender, ForwardPanelEventArgs e)
        {
            _forwardPanelEvent = e.Forward;
        }

        public bool ForwardPanelEvent
        {
            get => _forwardPanelEvent;
            set => _forwardPanelEvent = value;
        }

        public int VendorId
        {
            get => _vendorId;
            set => _vendorId = value;
        }

        public int ProductId
        {
            get => _productId;
            set => _productId = value;
        }

        public string InstanceId
        {
            get => HIDSkeletonBase.InstanceId;
            set => HIDSkeletonBase.InstanceId = value;
        }

        public string Hash => _hash;

        public string GuidString => _guid.ToString();

        public void SetLastException(Exception ex)
        {
            try
            {
                if (ex == null)
                {
                    return;
                }
                Common.LogError(666, ex, "Via GamingPanel.SetLastException()");
                lock (_exceptionLockObject)
                {
                    _lastException = new Exception(ex.GetType() + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
                }
            }
            catch (Exception)
            {
            }
        }


        /*         
         * These are used for static handling, the OnReport method must be static and to access the object that the OnReport belongs to must go via these methods.
         * If a static object would be kept inside the class it would go nuts considering there can be many panels of same type
         */
        public static void AddGamingPanelObject(GamingPanel gamingPanel)
        {
            lock (_lockObject)
            {
                GamingPanels.Add(gamingPanel);
            }
        }

        public static void RemoveGamingPanelObject(GamingPanel gamingPanel)
        {
            lock (_lockObject)
            {
                GamingPanels.Remove(gamingPanel);
            }
        }

        public static GamingPanel GetGamingPanelObject(string instanceId)
        {
            lock (_lockObject)
            {
                foreach (var gamingPanel in GamingPanels)
                {
                    if (gamingPanel.InstanceId.Equals(instanceId))
                    {
                        return gamingPanel;
                    }
                }
            }
            return null;
        }

        public Exception GetLastException(bool resetException = false)
        {
            Exception result;
            lock (_exceptionLockObject)
            {
                result = _lastException;
                if (resetException)
                {
                    _lastException = null;
                }
            }
            return result;
        }

        public bool IsDirty
        {
            get => _isDirty;
            set => _isDirty = value;
        }

        public bool SettingsLoading
        {
            get => _settingsLoading;
            set => _settingsLoading = value;
        }

        public GamingPanelEnum TypeOfGamingPanel
        {
            get => _typeOfGamingPanel;
            set => _typeOfGamingPanel = value;
        }
        //TODO fixa att man kan koppla in/ur panelerna?
        /*
         * 
        
        public bool IsAttached
        {
            get { return _isAttached; }
            set { _isAttached = value; }
        }
        */

        public bool Closed
        {
            get => _closed;
            set => _closed = value;
        }

        protected static bool FlagHasChanged(byte oldValue, byte newValue, int bitMask)
        {
            /*  --------------------------------------- 
             *  Example #1
             *  Old value 10110101
             *  New value 10110001
             *  Bit mask  00000100  <- This is the one we are interested in to see whether it has changed
             *  ---------------------------------------
             *  
             *  XOR       10110101
             *  ^         10110001
             *            --------
             *            00000100   <- Here are the bit(s) that has changed between old & new value
             *            
             *  AND       00000100
             *  &         00000100
             *            --------
             *            00000100   <- This shows that the value for this mask has changed since last time. Now get what is it (ON/OFF) using FlagValue function
             */

            /*  --------------------------------------- 
             *  Example #2
             *  Old value 10110101
             *  New value 10100101
             *  Bit mask  00000100  <- This is the one we are interested in to see whether it has changed
             *  ---------------------------------------
             *  
             *  XOR       10110101
             *  ^         10100101
             *            --------
             *            00010000   <- Here are the bit(s) that has changed between old & new value
             *            
             *  AND       00010000
             *  &         00000100
             *            --------
             *            00000000   <- This shows that the value for this mask has NOT changed since last time.
             */
            return ((oldValue ^ newValue) & bitMask) > 0;
        }

    }
    
    public class DCSBIOSUpdatesMissedEventArgs : EventArgs
    {
        public string UniqueId { get; set; }
        public GamingPanelEnum GamingPanelEnum { get; set; }
        public int Count { get; set; }
    }

    public class PanelEventArgs : EventArgs
    {
        public string UniqueId { get; set; }
        public GamingPanelEnum GamingPanelEnum { get; set; }
    }

    public class PanelDataToDCSBIOSEventEventArgs : EventArgs
    {
        public string StringData { get; set; }
    }

    public class SwitchesChangedEventArgs : EventArgs
    {
        public string UniqueId { get; set; }
        public GamingPanelEnum GamingPanelEnum { get; set; }
        public HashSet<object> Switches { get; set; }
    }

    public class ForwardPanelEventArgs : EventArgs
    {
        public bool Forward { get; set; }
    }
}