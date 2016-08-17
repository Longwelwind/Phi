using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace PhiClient.UI
{
    public class TabsContainer : Displayable
    {
        const float TAB_HEIGHT = 45f;

        List<TabEntry> tabs = new List<TabEntry>();
        Action<int> onChange;
        int selectedTab;

        public TabsContainer(int selectedTab, Action<int> onChange)
        {
            this.selectedTab = selectedTab;
            this.onChange = onChange;
        }

        public void AddTab(string label, Displayable displayable)
        {
            int index = tabs.Count;
            TabRecord tab = new TabRecord(label, () => onChange(index), selectedTab == index);
            tabs.Add(new TabEntry { tab = tab, displayable = displayable });
        }

        public override void Draw(Rect inRect)
        {
            // We draw the top with tabs
            Rect childArea = inRect.BottomPartPixels(inRect.height - TAB_HEIGHT);
            TabDrawer.DrawTabs(childArea, tabs.Select((e) => e.tab));

            // We draw the selected tab
            Displayable selectedDisplayable = tabs[selectedTab].displayable;

            selectedDisplayable.Draw(childArea);
        }
    }

    struct TabEntry
    {
        public TabRecord tab;
        public Displayable displayable;
    }
}
