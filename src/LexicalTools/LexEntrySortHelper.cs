using System;
using System.Collections.Generic;
using System.Globalization;
using WeSay.Data;
using WeSay.LexicalModel;
using WeSay.LexicalModel.Db4o_Specific;

namespace WeSay.LexicalTools
{
    public class LexEntrySortHelper: ISortHelper<string, LexEntry>
    {
        Db4oDataSource _db4oData; // for data
        string _writingSystemId;
        bool _isWritingSystemIdUsedByLexicalForm;
        
        public LexEntrySortHelper(Db4oDataSource db4oData, 
                                  string writingSystemId,
                                  bool isWritingSystemIdUsedByLexicalForm)
        {
            if(db4oData == null)
            {
                throw new ArgumentNullException("db4oData");
            }
            if (writingSystemId == null)
            {
                throw new ArgumentNullException("writingSystemId");
            }
            
            _db4oData = db4oData;
            _writingSystemId = writingSystemId;
            _isWritingSystemIdUsedByLexicalForm = isWritingSystemIdUsedByLexicalForm;
        }

        public LexEntrySortHelper(string writingSystemId,
                          bool isWritingSystemIdUsedByLexicalForm)
        {
            if (writingSystemId == null)
            {
                throw new ArgumentNullException("writingSystemId");
            }

            _writingSystemId = writingSystemId;
            _isWritingSystemIdUsedByLexicalForm = isWritingSystemIdUsedByLexicalForm;
        }

        #region IDb4oSortHelper<string,LexEntry> Members

        public IComparer<string> KeyComparer
        {
            get
            {
                StringComparer comparer;
                CultureInfo ci = null;
                try
                {
                    ci = CultureInfo.GetCultureInfo(_writingSystemId);
                }
                catch (ArgumentException e)
                {
                    if (e is ArgumentNullException ||
                        e is ArgumentOutOfRangeException)
                    {
                        throw;
                    }
                    if (Environment.OSVersion.Platform != PlatformID.Unix) 
                    {
                        ci = TryGetCultureInfoByIetfLanguageTag(ci); // not supported by mono yet
                    }
                }

                if (ci != null)
                {
                    comparer = StringComparer.Create(ci, false);
                }
                else
                {
                    comparer = StringComparer.InvariantCulture;
                }

                return comparer;
            }
        }

        private CultureInfo TryGetCultureInfoByIetfLanguageTag(CultureInfo ci)
        {
            try
            {
                ci = CultureInfo.GetCultureInfoByIetfLanguageTag(this._writingSystemId);
            }
            catch (ArgumentException ex)
            {
                if (ex is ArgumentNullException ||
                    ex is ArgumentOutOfRangeException)
                {
                    throw;
                }
            }
            return ci;
        }

        public List<KeyValuePair<string, long>> GetKeyIdPairs()
        {
            if (_db4oData != null)
            {
                if (_isWritingSystemIdUsedByLexicalForm)
                {
                    return KeyToEntryIdInitializer.GetLexicalFormToEntryIdPairs(_db4oData,
                                                                                _writingSystemId);
                }
                else
                {
                    return KeyToEntryIdInitializer.GetGlossToEntryIdPairs(_db4oData,
                                                                          _writingSystemId);
                }
            }
            throw new InvalidOperationException();
        }

        public IEnumerable<string> GetKeys(LexEntry item)
        {
            List<string> keys = new List<string>();
            if (_isWritingSystemIdUsedByLexicalForm)
            {
                keys.Add(item.LexicalForm.GetBestAlternative(_writingSystemId, "*"));
            }
            else
            {
                bool hasSense = false;
                foreach (LexSense sense in item.Senses)
                {
                    hasSense = true;

                    keys.AddRange(KeyToEntryIdInitializer.SplitGlossAtSemicolon(sense.Gloss, _writingSystemId));
                }
                if(!hasSense)
                {
                    keys.Add("*");
                }
            }
            return keys;
        }


        public string Name
        {
            get
            {
                return "LexEntry sorted by " + _writingSystemId;
            }
        }

        #endregion
    }
}
