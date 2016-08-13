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
        int selectedTab;

        public TabsContainer(int selectedTab, Action onChange)
        {
            this.selectedTab = selectedTab;
        }

        public void AddTab(string label, Displayable displayable)
        {
            int index = tabs.Count;
            TabRecord tab = new TabRecord(label, () => selectedTab = index, selectedTab == index);
            tabs.Add(new TabEntry { tab = tab, displayable = displayable });
        }

        public override void Draw(Rect inRect)
        {
            // We draw the top with tabs
            Rect tabsArea = inRect.TopPartPixels(TAB_HEIGHT);
            TabDrawer.DrawTabs(tabsArea, tabs.Select((e) => e.tab));

            // We draw the selected tab
            Rect childArea = inRect.BottomPartPixels(inRect.height - TAB_HEIGHT);
            Displayable selectedDisplayable = tabs[selectedTab].displayable;

            selectedDisplayable.Draw(childArea);
        }

        public override float CalcHeight(float width)
        {
            return TAB_HEIGHT;
        }

        public override bool IsFluidHeight()
        {
            return false;
        }
    }

    struct TabEntry
    {
        public TabRecord tab;
        public Displayable displayable;
    }
}
