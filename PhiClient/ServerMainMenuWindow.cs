using PhiClient.UI;
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

        Vector2 scrollPosition = Vector2.zero;

        public override void DoWindowContents(Rect inRect)
        {
            PhiClient client = PhiClient.instance;

            ListContainer cont = new ListContainer();
            cont.Add(new HeightContainer(DoHeader(), 30f));

            ListContainer messages = new ListContainer();
            ScrollContainer scroll = new ScrollContainer(messages, scrollPosition, (s) => { scrollPosition = s; });

            for (int i = 0; i < 100; i++)
            {
                messages.Add(new TextWidget("message" + i));
            }


            cont.Add(scroll);

            /*if (client.IsUsable())
            {
                DrawConnectedContent(inRect);
            }
            else
            {
                DrawnDisconnectedContent(inRect);
            }*/

            cont.Draw(inRect);
        }

        string enteredAddress = "";

        public Displayable DoHeader()
        {
            PhiClient client = PhiClient.instance;
            ListContainer cont = new ListContainer(ListFlow.ROW);
            cont.spaceBetween = ListContainer.SPACE;

            if (client.IsUsable())
            {

            }
            else
            {
                cont.Add(new TextFieldWidget(enteredAddress, (s) => { enteredAddress = s; }));
                cont.Add(new ButtonWidget("Connect", () => { OnConnectButtonClick(); }));
            }

            return cont;
        }

        const float PREFERENCE_ROW_HEIGHT = 30f;
        const float PREFERENCE_ROW_WIDTH = 200f;

        public void DrawConnectedContent(Rect inRect)
        {
            PhiClient client = PhiClient.instance;

            Widgets.Label(inRect, "Connected");

            Rect preferencesArea = inRect.BottomPartPixels(inRect.height - 30f - ListContainer.SPACE);
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

        public void DrawnDisconnectedContent(Rect inRect)
        {
            /**
             * Input for the server address
             */
            Rect topArea = inRect.TopPartPixels(CONNECT_HEIGHT);
            Rect inputArea = topArea.LeftPartPixels(inRect.width - CONNECT_BUTTON_WIDTH - ListContainer.SPACE);
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
