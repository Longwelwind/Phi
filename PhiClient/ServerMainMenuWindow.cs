using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace PhiClient
{
    class ServerMainMenuWindow : Window
    {
        const float MARGIN = 8f;

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(600, 600);
            }
        }

        public override void PreOpen()
        {
            base.PreOpen();

            this.enteredAddress = PhiClient.instance.GetServerAddress();
        }

        public override void DoWindowContents(Rect inRect)
        {
            PhiClient client = PhiClient.instance;

            if (client.IsUsable())
            {
                DrawConnectedContent(inRect);
            }
            else
            {
                DrawnDisconnectedContent(inRect);
            }
        }

        const float PREFERENCE_ROW_HEIGHT = 30f;
        const float PREFERENCE_ROW_WIDTH = 200f;

        public void DrawConnectedContent(Rect inRect)
        {
            PhiClient client = PhiClient.instance;

            Widgets.Label(inRect, "Connected");

            Rect preferencesArea = inRect.BottomPartPixels(inRect.height - 30f - MARGIN);
            Rect rowArea = preferencesArea.LeftPartPixels(PREFERENCE_ROW_WIDTH).TopPartPixels(PREFERENCE_ROW_HEIGHT);

            bool oldReceiveItems = client.currentUser.preferences.receiveItems;
            Widgets.CheckboxLabeled(rowArea, "Receive items", ref client.currentUser.preferences.receiveItems);
            if (oldReceiveItems != client.currentUser.preferences.receiveItems)
            {
                // The preference changed
                client.UpdatePreferences();
            }
        }

        const float CONNECT_BUTTON_WIDTH = 150f;
        const float CONNECT_HEIGHT = 40f;
        string enteredAddress = "";

        public void DrawnDisconnectedContent(Rect inRect)
        {
            /**
             * Input for the server address
             */
            Rect topArea = inRect.TopPartPixels(CONNECT_HEIGHT);
            Rect inputArea = topArea.LeftPartPixels(inRect.width - CONNECT_BUTTON_WIDTH - MARGIN);
            Rect connectButton = topArea.RightPartPixels(CONNECT_BUTTON_WIDTH);

            enteredAddress = Widgets.TextField(inputArea, enteredAddress);
            if (Widgets.ButtonText(connectButton, "Connect"))
            {
                this.OnConnectButtonClick();
            }
        }

        public void OnConnectButtonClick()
        {
            PhiClient client = PhiClient.instance;

            client.SetServerAddress(enteredAddress);
            client.TryConnect();
        }
    }
}
