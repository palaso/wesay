using NUnit.Framework;
using Palaso.WritingSystems;
using WeSay.LexicalModel.Foundation;

namespace WeSay.LexicalModel.Tests.Foundation
{
	[TestFixture]
	public class WritingSystemTests
	{
		[Test]
		public void NoSetupDefaultFont()
		{
			var ws = WritingSystemDefinition.FromLanguage("xx");
			Assert.AreEqual(33, WritingSystemInfo.CreateFont(ws).Size);
		}

		[Test]
		public void Construct_DefaultFont()
		{
			var ws = new WritingSystemDefinition();
			Assert.IsNotNull(WritingSystemInfo.CreateFont(ws));
		}

		[Test]
		public void Compare_fr_sortsLikeFrench()
		{
			var writingSystem = WritingSystemDefinition.FromLanguage("one");
			writingSystem.SortUsingOtherLanguage("fr");
			//u00c8 is Latin Capital Letter E with Grave
			//u00ed is Latin small letter i with acute

			Assert.Less(writingSystem.Collator.Compare("\u00c8dit", "Ed\u00edt"), 0);
		}

		[Test]
		public void Compare_en_sortsLikeEnglish()
		{
			var writingSystem = WritingSystemDefinition.FromLanguage("one");
			writingSystem.SortUsingOtherLanguage("en-US");
			//u00c8 is Latin Capital Letter E with Grave
			//u00ed is Latin small letter i with acute
			Assert.Greater(writingSystem.Collator.Compare("\u00c8dit", "Ed\u00edt"), 0);
		}

		[Test]
		public void Constructor_IsAudio_SetToFalse()
		{
			var writingSystem = WritingSystemDefinition.FromLanguage("one");
			Assert.IsFalse(writingSystem.IsVoice);
		}

		[Test]
		public void Constructor_IsUnicode_SetToTrue()
		{
			var writingSystem = WritingSystemDefinition.FromLanguage("one");
			Assert.IsTrue(writingSystem.IsUnicodeEncoded);
		}

		[Test, Ignore]
		public void SortUsing_CustomSimpleWithNoRules_sortsLikeInvariant()
		{
			var writingSystem = WritingSystemDefinition.FromLanguage("one");
			writingSystem.SortUsingCustomSimple("");
			// hard to test because half of the system locales use the invariant table: http://blogs.msdn.com/michkap/archive/2004/12/29/344136.aspx
		}

		[Test]
		public void SortUsingOtherLanguage_Null_SetToId()
		{
			// Not convinced that this needs to be true. Given that the sort method is known to be OtherLanguage then
			// the implementation can just ignore sort rules and use the id instead.
			var writingSystem = WritingSystemDefinition.FromLanguage("one");
			writingSystem.SortUsingOtherLanguage(null);
			Assert.AreEqual(writingSystem.Id, writingSystem.SortRules);
		}

		[Test]
		public void SortUsingCustomICU_WithSortRules_SetsSortRulesAndSortUsing()
		{
			const string rules = "&n < ng <<< Ng <<< NG";
			WritingSystemDefinition writingSystem = WritingSystemDefinition.FromLanguage("one");
			writingSystem.SortUsingCustomICU(rules);
			Assert.AreEqual(rules, writingSystem.SortRules);
			Assert.AreEqual(WritingSystemDefinition.SortRulesType.CustomICU, writingSystem.SortUsing);
		}

		[Test]
		public void GetHashCode_SameIdDefaultsDifferentFont_Same()
		{
			WritingSystemDefinition writingSystem1 = WritingSystemDefinition.FromLanguage("ws");
			writingSystem1.DefaultFontName = "Arial";
			writingSystem1.DefaultFontSize = 12;
			WritingSystemDefinition writingSystem2 = WritingSystemDefinition.FromLanguage("ws");
			writingSystem2.DefaultFontName = "Arial";
			writingSystem2.DefaultFontSize = 22;

			Assert.AreEqual(writingSystem1.GetHashCode(), writingSystem2.GetHashCode());
		}

		[Test]
		public void GetHashCode_SameIdSortUsingNoCustomRules_Same()
		{
			WritingSystemDefinition writingSystem1 = WritingSystemDefinition.FromLanguage("ws");
			writingSystem1.SortUsingOtherLanguage("th");
			WritingSystemDefinition writingSystem2 = WritingSystemDefinition.FromLanguage("ws");
			writingSystem2.SortUsingOtherLanguage("th");

			Assert.AreEqual(writingSystem1.GetHashCode(), writingSystem2.GetHashCode());
		}

		[Test]
		public void GetHashCode_SameIdSortUsingCustomRules_Same()
		{
			WritingSystemDefinition writingSystem1 = WritingSystemDefinition.FromLanguage("ws");
			writingSystem1.SortUsingCustomSimple("A");

			WritingSystemDefinition writingSystem2 = WritingSystemDefinition.FromLanguage("ws");
			writingSystem1.SortUsingCustomSimple("A");

			Assert.AreEqual(writingSystem1.GetHashCode(), writingSystem2.GetHashCode());
		}

		[Test]
		public void GetHashCode_DifferentId_Different()
		{
			WritingSystemDefinition writingSystem1 = WritingSystemDefinition.FromLanguage("ws");
			WritingSystemDefinition writingSystem2 = WritingSystemDefinition.FromLanguage("sw");

			Assert.AreNotEqual(writingSystem1.GetHashCode(), writingSystem2.GetHashCode());
		}

		[Test]
		public void GetHashCode_DifferentSortUsing_Different()
		{
			WritingSystemDefinition writingSystem1 = WritingSystemDefinition.FromLanguage("ws");
			writingSystem1.SortUsingOtherLanguage("th");
			WritingSystemDefinition writingSystem2 = WritingSystemDefinition.FromLanguage("ws");
			writingSystem1.SortUsingOtherLanguage("th-TH");

			Assert.AreNotEqual(writingSystem1.GetHashCode(), writingSystem2.GetHashCode());
		}

		[Test]
		public void GetHashCode_DifferentCustomSortRuleTypes_Different()
		{
			var writingSystem1 = WritingSystemDefinition.FromLanguage("ws");
			writingSystem1.SortUsingCustomSimple("A");

			var writingSystem2 = WritingSystemDefinition.FromLanguage("ws");
			writingSystem2.SortUsingCustomICU("A");

			Assert.AreNotEqual(writingSystem1.GetHashCode(), writingSystem2.GetHashCode());
		}

		[Test]
		public void GetHashCode_DifferentCustomSortRules_Different()
		{
			var writingSystem1 = WritingSystemDefinition.FromLanguage("ws");
			writingSystem1.SortUsingCustomSimple("A");

			var writingSystem2 = WritingSystemDefinition.FromLanguage("ws");
			writingSystem2.SortUsingCustomSimple("A a");

			Assert.AreNotEqual(writingSystem1.GetHashCode(), writingSystem2.GetHashCode());
		}

		[Test]
		public void GetSpellCheckingId_Uninitialized_ReturnsId()
		{
			var writingSystem = new WritingSystemDefinition();
			writingSystem.ISO639 = "en";
			Assert.AreEqual("en", writingSystem.SpellCheckingId);
		}

		[Test]
		public void GetAbbreviation_Uninitialized_ReturnsId()
		{
			var writingSystem = new WritingSystemDefinition();
			writingSystem.ISO639 = "en";
			Assert.AreEqual("en", writingSystem.Abbreviation);
		}

		[Test]
		public void GetSpellcheckingId_SpellcheckingIdIsSet_ReturnsSpellCheckingId()
		{
			var writingSystem = new WritingSystemDefinition();
			writingSystem.SpellCheckingId = "en_US";
			Assert.AreEqual("en_US", writingSystem.SpellCheckingId);
		}

		[Test]
		public void GetAbbreviation_AbbreviationIsSet_ReturnsAbbreviation()
		{
			// Expect that this will now throw! en should preferred over eng
			var writingSystem = new WritingSystemDefinition();
			writingSystem.Abbreviation = "eng";
			Assert.AreEqual("eng", writingSystem.Abbreviation);
		}

	}
}