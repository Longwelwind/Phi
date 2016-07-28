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

            this.enteredAddress = PhiClient.instance.serverAddress;
        }

        Vector2 scrollPosition = Vector2.zero;

        public override void DoWindowContents(Rect inRect)
        {
            PhiClient client = PhiClient.instance;

            ListContainer cont = new ListContainer();
            cont.Add(new HeightContainer(DoHeader(), 30f));

            if (client.IsUsable())
            {
                cont.Add(DoConnectedContent());
            }

            cont.Draw(inRect);
        }

        public Displayable DoConnectedContent()
        {
            PhiClient client = PhiClient.instance;
            UserPreferences pref = client.currentUser.preferences;

            ListContainer twoColumn = new ListContainer(ListFlow.ROW);
            twoColumn.spaceBetween = ListContainer.SPACE;

            ListContainer firstColumn = new ListContainer();
            twoColumn.Add(firstColumn);

            firstColumn.Add(new CheckboxLabeledWidget("Allow receiving items", pref.receiveItems, (b) =>
            {
                pref.receiveItems = b;
                client.UpdatePreferences();
            }));

            // Just to take spaces while the column is empty
            ListContainer secondColumn = new ListContainer();
            twoColumn.Add(secondColumn);

            return twoColumn;
        }

        string enteredAddress = "";

        public Displayable DoHeader()
        {
            PhiClient client = PhiClient.instance;
            ListContainer cont = new ListContainer(ListFlow.ROW);
            cont.spaceBetween = ListContainer.SPACE;

            if (client.IsUsable())
            {
                cont.Add(new TextWidget("Connected to" + client.serverAddress, GameFont.Small, TextAnchor.MiddleLeft));
                cont.Add(new WidthContainer(new ButtonWidget("Disconnected", () => { OnConnectButtonClick(); }), 120f));
            }
            else
            {
                cont.Add(new TextFieldWidget(enteredAddress, (s) => { enteredAddress = s; }));
                cont.Add(new WidthContainer(new ButtonWidget("Connect", () => { OnConnectButtonClick(); }), 120f));
            }

            return cont;
        }

        public void OnConnectButtonClick()
        {
            PhiClient client = PhiClient.instance;

            client.SetServerAddress(enteredAddress);
            client.TryConnect();
        }
    }
}
