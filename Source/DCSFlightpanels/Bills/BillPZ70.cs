﻿using System;
using System.Collections.Generic;
using System.Windows.Media;
using DCS_BIOS;
using DCSFlightpanels.CustomControls;
using NonVisuals.DCSBIOSBindings;
using NonVisuals.Saitek;

namespace DCSFlightpanels.Bills
{
    public class BillPZ70 : BillBaseInput
    {
        private MultiPanelPZ70KnobOnOff _key;
        private DCSBIOSActionBindingPZ70 _dcsbiosBindingPZ70;
        private BIPLinkPZ70 _bipLinkPZ70;

        public BillPZ70(PZ70TextBox textBox, MultiPanelPZ70KnobOnOff key)
        {
            TextBox = textBox;
            _key = key;
        }

        public override bool ContainsDCSBIOS()
        {
            return _dcsbiosBindingPZ70 != null;// && _dcsbiosInputs.Count > 0;
        }

        public override bool ContainsBIPLink()
        {
            return _bipLinkPZ70 != null && _bipLinkPZ70.BIPLights.Count > 0;
        }

        public override bool IsEmpty()
        {
            return (_bipLinkPZ70 == null || _bipLinkPZ70.BIPLights.Count == 0) && (_dcsbiosBindingPZ70?.DCSBIOSInputs == null || _dcsbiosBindingPZ70.DCSBIOSInputs.Count == 0) && (KeyPress == null || KeyPress.KeySequence.Count == 0);
        }

        public override void Consume(List<DCSBIOSInput> dcsBiosInputs)
        {
            if (_dcsbiosBindingPZ70 == null)
            {
                _dcsbiosBindingPZ70 = new DCSBIOSActionBindingPZ70();
            }

            _dcsbiosBindingPZ70.DCSBIOSInputs = dcsBiosInputs;
        }

        public DCSBIOSActionBindingPZ70 DCSBIOSBinding
        {
            get => _dcsbiosBindingPZ70;
            set
            {
                if (ContainsKeyPress())
                {
                    throw new Exception("Cannot insert DCSBIOSInputs, Bill already contains KeyPress");
                }
                _dcsbiosBindingPZ70 = value;
                if (_dcsbiosBindingPZ70 != null)
                {
                    if (string.IsNullOrEmpty(_dcsbiosBindingPZ70.Description))
                    {
                        TextBox.Text = "DCS-BIOS";
                    }
                    else
                    {
                        TextBox.Text = _dcsbiosBindingPZ70.Description;
                    }
                }
                else
                {
                    TextBox.Text = "";
                }
            }
        }

        public BIPLinkPZ70 BIPLink
        {
            get => _bipLinkPZ70;
            set
            {
                _bipLinkPZ70 = value;
                if (_bipLinkPZ70 != null)
                {
                    TextBox.Background = Brushes.Bisque;
                }
                else
                {
                    TextBox.Background = Brushes.White;
                }
            }
        }
        
        public MultiPanelPZ70KnobOnOff Key
        {
            get => _key;
            set => _key = value;
        }

        public override void Clear()
        {
            _dcsbiosBindingPZ70 = null;
            _bipLinkPZ70 = null;
            KeyPress = null;
            TextBox.Background = Brushes.White;
            TextBox.Text = "";
        }
    }
}
