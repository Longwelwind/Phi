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

        string messageToSend = "";

        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);

            /**
             * Drawing the title
             */
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.UpperCenter;
            string title = "Realm";
            float titleHeight = Text.CalcHeight(title, inRect.width);
            Widgets.Label(new Rect(0f, 0f, inRect.width, 300f), title);

            Rect contentArea = inRect.BottomPartPixels(inRect.height - titleHeight);
            Rect chatArea = contentArea.LeftPartPixels(inRect.width - STATUS_AREA_WIDTH);
            Rect statusArea = contentArea.RightPartPixels(STATUS_AREA_WIDTH);

            this.DrawChat(chatArea);
            this.DrawStatus(statusArea);

            /**
             * Cleanup global state
             */
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DrawStatus(Rect rect)
        {
            PhiClient phiClient = PhiClient.instance;

            /**
             * Drawing the status of the connection
             */
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            string status = "Status: ";
            switch (phiClient.client.state)
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

            float textHeight = Text.CalcHeight(status, rect.width);
            Widgets.Label(rect, status);
            rect = rect.BottomPartPixels(rect.height - textHeight);

            /**
             * Drawing the reconnection button
             */
            if (!phiClient.IsConnected())
            {
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.UpperLeft;

                Rect reconnectButtonArea = rect.TopPartPixels(40);

                if (Widgets.ButtonText(reconnectButtonArea, "Reconnect", true, true))
                {
                    this.OnReconnectClick();
                }

                rect = rect.BottomPartPixels(rect.height - reconnectButtonArea.height);
            }

            /**
             * Drawing the list of users
             */
            if (phiClient.IsUsable())
            {
                // Ordering the list according to connected status
                List<User> users = phiClient.realmData.users.OrderBy(u => u.connected).ToList();

                foreach (User user in users)
                {
                    string line = user.name;
                    if (user.connected)
                    {
                        line = "• " + line;
                    }
                    textHeight = Text.CalcHeight(line, rect.width);

                    // We check if we have not run out of space
                    if (rect.height < textHeight)
                    {
                        break;
                    }

                    Rect buttonArea = rect.TopPartPixels(textHeight);
                    if (Widgets.ButtonText(buttonArea, line, false))
                    {
                        this.OnUserClick(user);
                    }

                    rect = rect.BottomPartPixels(rect.height - buttonArea.height);
                }
            }
        }

        private void DrawChat(Rect rect)
        {
            PhiClient phiClient = PhiClient.instance;
            Rect messagesArea = rect.TopPartPixels(rect.height - CHAT_INPUT_HEIGHT).ContractedBy(CHAT_MARGIN);
            Rect inputArea = rect.BottomPartPixels(CHAT_INPUT_HEIGHT);

            /**
             * Drawing the messages
             */
            if (phiClient.IsUsable())
            {
                List<ChatMessage> messages = PhiClient.instance.realmData.chat;

                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.UpperLeft;

                // We fill the chat by the bottom, inverting the order of messages
                Rect messageSlot = messagesArea.BottomPart(0);
                foreach (ChatMessage message in messages.AsEnumerable().Reverse())
                {
                    string entry = message.user.name + ": " + message.message;

                    float height = Text.CalcHeight(entry, messageSlot.width);
                    messageSlot.y -= height;
                    messageSlot.height = height;
                    Widgets.Label(messageSlot, entry);
                }
            }

            /**
             * Drawing the chat input TextField and Button
             */
            Rect textFieldArea = inputArea.LeftPartPixels(inputArea.width - CHAT_INPUT_SEND_BUTTON_WIDTH);
            Rect sendButtonArea = inputArea.RightPartPixels(CHAT_INPUT_SEND_BUTTON_WIDTH);

            messageToSend = Widgets.TextField(textFieldArea, messageToSend);
            if (Widgets.ButtonText(sendButtonArea, "Send", true, true))
            {
                this.OnSendClick();
            }
        }

        public void OnSendClick()
        {
            PhiClient.instance.SendMessage(this.messageToSend);
            this.messageToSend = "";
        }

        public void OnReconnectClick()
        {
            PhiClient.instance.TryConnect();
        }

        public void OnUserClick(User user)
        {
            PhiClient phiClient = PhiClient.instance;
            // We open a trade window with this user
            if (user != phiClient.currentUser || true)
            {
                Find.WindowStack.Add(new UserGiveWindow(user));
            }
        }
    }
}
