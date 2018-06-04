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
        public ServerMainMenuWindow()
        {
            this.doCloseX = true;
            this.closeOnClickedOutside = true;
            this.closeOnEscapeKey = true;
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(700, 700);
            }
        }

        public override void PreOpen()
        {
            base.PreOpen();

            PhiClient client = PhiClient.instance;

            this.enteredAddress = client.serverAddress;

            if (client.IsUsable())
            {
                OnUsableCallback();
            }
            client.OnUsable += OnUsableCallback;
        }

        public override void PostClose()
        {
            base.PostClose();

            PhiClient client = PhiClient.instance;
            client.OnUsable -= OnUsableCallback;
        }

        void OnUsableCallback()
        {
            this.wantedNickname = PhiClient.instance.currentUser.name;
        }

        Vector2 scrollPosition = Vector2.zero;

        public override void DoWindowContents(Rect inRect)
        {
            PhiClient client = PhiClient.instance;

            ListContainer cont = new ListContainer();
            cont.spaceBetween = ListContainer.SPACE;
            cont.Add(new HeightContainer(DoHeader(), 30f));

            if (client.IsUsable())
            {
                cont.Add(DoConnectedContent());
            }

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
                cont.Add(new TextWidget("Connected to " + client.serverAddress, GameFont.Small, TextAnchor.MiddleLeft));
                cont.Add(new WidthContainer(new ButtonWidget("Disconnect", () => { OnDisconnectButtonClick(); }), 140f));
            }
            else
            {
                cont.Add(new TextFieldWidget(enteredAddress, (s) => { enteredAddress = s; }));
                cont.Add(new WidthContainer(new ButtonWidget("Connect", () => { OnConnectButtonClick(); }), 140f));
            }

            return cont;
        }

        string wantedNickname;

        public Displayable DoConnectedContent()
        {
            PhiClient client = PhiClient.instance;
            ListContainer mainCont = new ListContainer();
            mainCont.spaceBetween = ListContainer.SPACE;

            /**
             * Changing your nickname
             */
            ListContainer changeNickCont = new ListContainer(ListFlow.ROW);
            changeNickCont.spaceBetween = ListContainer.SPACE;
            mainCont.Add(new HeightContainer(changeNickCont, 30f));
            
            changeNickCont.Add(new TextFieldWidget(wantedNickname, (s) => wantedNickname = OnWantedNicknameChange(s)));
            changeNickCont.Add(new WidthContainer(new ButtonWidget("Change nickname", OnChangeNicknameClick), 140f));

            /**
             * Preferences list
             */
            UserPreferences pref = client.currentUser.preferences;
            ListContainer twoColumn = new ListContainer(ListFlow.ROW);
            twoColumn.spaceBetween = ListContainer.SPACE;
            mainCont.Add(twoColumn);

            ListContainer firstColumn = new ListContainer();
            twoColumn.Add(firstColumn);

            firstColumn.Add(new CheckboxLabeledWidget("Allow receiving items", pref.receiveItems, (b) =>
            {
                pref.receiveItems = b;
                client.UpdatePreferences();
            }));

            firstColumn.Add(new CheckboxLabeledWidget("Allow receiving colonists (EXPERIMENTAL)", pref.receiveColonists, (b) =>
            {
                pref.receiveColonists = b;
                client.UpdatePreferences();
            }));

            firstColumn.Add(new CheckboxLabeledWidget("Allow receiving animals (EXPERIMENTAL)", pref.receiveAnimals, (b) =>
            {
                pref.receiveAnimals = b;
                client.UpdatePreferences();
            }));

            // Just to take spaces while the column is empty
            ListContainer secondColumn = new ListContainer();
            twoColumn.Add(secondColumn);

            return mainCont;
        }

        public string OnWantedNicknameChange(string newNickname)
        {
            // TODO: Limit the length of the nickname right in the interface
            return newNickname;
        }

        public void OnConnectButtonClick()
        {
            PhiClient client = PhiClient.instance;

            client.SetServerAddress(enteredAddress.Trim());
            client.TryConnect();
        }

        public void OnDisconnectButtonClick()
        {
            PhiClient.instance.Disconnect();
        }

        void OnChangeNicknameClick()
        {
            PhiClient.instance.ChangeNickname(wantedNickname);
        }
    }
}
