# -*- coding: utf-8 -*-
"""
Created on Sun Feb 21 22:20:06 2021

@author: gandg
"""

import string
import gensim
import smart_open
from gensim.models.doc2vec import Doc2Vec

def read_corpus(fname):
    with smart_open.open(fname, encoding="utf8") as f:
        strData = f.read()
        sentences = strData.split('<dmz>')
        for i in range(len(sentences)):
            tokens = gensim.utils.simple_preprocess(sentences[i])
            yield gensim.models.doc2vec.TaggedDocument(tokens, [i])
                
         

title_train_corpus = list(read_corpus('D:\\wamp\\tmp\\titlesData.txt'))
titleModel = Doc2Vec(title_train_corpus, vector_size=15, window=2, min_count=1, workers=8, epochs=40)
titleModel.save('D:\\wamp\\tmp\\titleModel.mod')

articleText_train_corpus = list(read_corpus('D:\\wamp\\tmp\\articleTextData.txt'))
articleTextModel = Doc2Vec(articleText_train_corpus, vector_size=70, window=2, min_count=1, workers=8, epochs=40)
articleTextModel.save('D:\\wamp\\tmp\\articleTextModel.mod')



#vector = model.infer_vector(['24', 'იანვრის', 'სატრანსფერო', 'ზონა'])

