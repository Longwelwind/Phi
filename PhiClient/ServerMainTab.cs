using PhiClient.UI;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using WebSocketSharp;

namespace PhiClient
{
    public class ServerMainTab : MainTabWindow
    {
        const float TITLE_HEIGHT = 45f;
        const float CHAT_INPUT_HEIGHT = 30f;
        const float CHAT_INPUT_SEND_BUTTON_WIDTH = 100f;
        const float CHAT_MARGIN = 10f;
        const float STATUS_AREA_WIDTH = 160f;

        string enteredMessage = "";

        string filterName = "";
        List<Thing> filteredUsers;

        public ServerMainTab()
        {
        }

        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);

            PhiClient phi = PhiClient.instance;

            ListContainer mainList = new ListContainer();
            mainList.spaceBetween = ListContainer.SPACE;
            
            mainList.Add(new TextWidget("Realm", GameFont.Medium, TextAnchor.MiddleCenter));

            ListContainer rowBodyContainer = new ListContainer(new List<Displayable>()
            {
                DoChat(),
                new WidthContainer(DoBodyRightBar(), STATUS_AREA_WIDTH)
            }, ListFlow.ROW);
            rowBodyContainer.spaceBetween = ListContainer.SPACE;

            mainList.Add(rowBodyContainer);
            mainList.Add(new HeightContainer(DoFooter(), 30f));

            mainList.Draw(inRect);
        }

        Vector2 chatScroll = Vector2.zero;

        private Displayable DoChat()
        {
            PhiClient phi = PhiClient.instance;

            var cont = new ListContainer(ListFlow.COLUMN, ListDirection.OPPOSITE);

            foreach (ChatMessage c in phi.realmData.chat.Reverse<ChatMessage>().Take(30))
            {
                cont.Add(new TextWidget(c.user.name + ": " + c.message));
            }

            return new ScrollContainer(cont, chatScroll, (v) => { chatScroll = v; });
        }

        Vector2 userScrollPosition = Vector2.zero;

        private Displayable DoBodyRightBar()
        {
            PhiClient phi = PhiClient.instance;

            ListContainer cont = new ListContainer();

            string status = "Status: ";
            switch (phi.client.state)
            {
                case WebSocketState.Open:
                    status += "Connected";
                    break;
                case WebSocketState.Closed:
                    status += "Disconnected";
                    break;
                case WebSocketState.Connecting:
                    status += "Connecting";
                    break;
                case WebSocketState.Closing:
                    status += "Disconnecting";
                    break;
            }
            cont.Add(new TextWidget(status));
            cont.Add(new Container(new TextFieldWidget(filterName, (s) => {
                filterName = s;
            }), 150f, 30f));

            if (phi.IsUsable())
            {
                ListContainer usersList = new ListContainer();
                foreach (User user in phi.realmData.users.Where((u) => u.connected))
                {
                    if (filterName != "")
                    {
                        if (ContainsStringIgnoreCase(user.name, filterName))
                            usersList.Add(new ButtonWidget(user.name, () => { OnUserClick(user); }, false));
                    } else
                    {
                        usersList.Add(new ButtonWidget(user.name, () => { OnUserClick(user); }, false));
                    }
                }

                cont.Add(new ScrollContainer(usersList, userScrollPosition, (v) => { userScrollPosition = v; }));
            }
            return cont;
        }

        private System.Boolean ContainsStringIgnoreCase(string hay, string needle)
        {
            return hay.IndexOf(needle, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private Displayable DoFooter()
        {
            ListContainer footerList = new ListContainer(ListFlow.ROW);
            footerList.spaceBetween = ListContainer.SPACE;

            if (Input.GetKeyDown(KeyCode.Return))
            {
                OnSendClick();
            }

            footerList.Add(new TextFieldWidget(enteredMessage, (s) => { enteredMessage = s; }));
            footerList.Add(new WidthContainer(new ButtonWidget("Send", OnSendClick), CHAT_INPUT_SEND_BUTTON_WIDTH));
            
            return footerList;
        }

        public void OnSendClick()
        {
            string trimmedMessage = this.enteredMessage.Trim();
            if (trimmedMessage.IsNullOrEmpty())
            {
                return;
            }
            PhiClient.instance.SendMessage(this.enteredMessage);
            this.enteredMessage = "";
        }

        public void OnReconnectClick()
        {
            PhiClient.instance.TryConnect();
        }

        public void OnUserClick(User user)
        {
            PhiClient phiClient = PhiClient.instance;

            if (user != phiClient.currentUser || true)
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                options.Add(new FloatMenuOption("Ship items", () => { OnShipItemsOptionClick(user); }));
                options.Add(new FloatMenuOption("Send colonist", () => { OnSendColonistOptionClick(user); }));

                Find.WindowStack.Add(new FloatMenu(options));
            }
        }

        public void OnSendColonistOptionClick(User user)
        {
            PhiClient phiClient = PhiClient.instance;
            // We open a trade window with this user
            if (user.preferences.receiveColonists)
            {
                Find.WindowStack.Add(new UserSendColonistWindow(user));
            }
            else
            {
                Messages.Message(user.name + " does not accept colonists", MessageSound.RejectInput);
            }
        }

        public void OnShipItemsOptionClick(User user)
        {
            PhiClient phiClient = PhiClient.instance;
            // We open a trade window with this user
            if (user.preferences.receiveItems)
            {
                Find.WindowStack.Add(new UserGiveWindow(user));
            }
            else
            {
                Messages.Message(user.name + " does not accept items", MessageSound.RejectInput);
            }
        }
    }
}
