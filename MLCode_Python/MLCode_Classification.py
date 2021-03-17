# -*- coding: utf-8 -*-
"""
Spyder Editor

This is a temporary script file.
"""
import ast
import numpy as np
import matplotlib.pyplot as plt
import pandas as pd
import pymongo 
import json
import keras
import keras.backend as K
import sklearn.metrics as metrics
import gensim
from keras.models import Sequential
from keras.layers import Dense
from sklearn.preprocessing import LabelEncoder, OneHotEncoder, StandardScaler, MinMaxScaler
from sklearn.compose import ColumnTransformer
from sklearn.model_selection import train_test_split
from operator import itemgetter
from matplotlib import pyplot as plt
from keras.wrappers.scikit_learn import KerasRegressor, KerasClassifier
from sklearn.model_selection import cross_val_score
from gensim.models.doc2vec import Doc2Vec


def build_classifier(input_size):
    
    hl_1_dim = int(input_size / 2)
    hl_2_dim = int(hl_1_dim / 2)
    
    classifier = Sequential()
    classifier.add(Dense(units = hl_1_dim, kernel_initializer = 'normal', activation='relu', input_dim = input_size))
    classifier.add(Dense(units = hl_2_dim, kernel_initializer = 'normal', activation='relu'))
    classifier.add(Dense(units = 1, kernel_initializer = 'normal', activation='sigmoid') )
    # = keras.optimizers.Adam(learning_rate=0.0001)
    classifier.compile(optimizer = 'adam', loss = 'binary_crossentropy', metrics=['accuracy'])
    return classifier

def soft_acc(y_true, y_pred):
    return K.mean(K.equal(K.round(y_true), K.round(y_pred)))

def addWordColumnsAndData(articles, articleWordOccurenceData):
   
    newlist = sorted(articles, key=itemgetter('_id'))
    k = 0
    prevId = -1
    
    for item in articleWordOccurenceData:
        columnName ='Word.' + item['Word'];
        value = item['Occurence']
        articleId = item['ArticleId']
        
        newlist[k][columnName] = value
        
        if (prevId != articleId):
            if (prevId == -1):
                prevId = articleId
            else:
                prevId = articleId
                k += 1
           
             
    return newlist

def addTitleModelProcessedData(articles):
    model = Doc2Vec.load('D:\\wamp\\tmp\\titleModel.mod')
    for i, article in enumerate(articles):
        tokens = gensim.utils.simple_preprocess(article['Title'])
        vector = model.infer_vector(tokens)
        for l, num in enumerate(vector):
            articles[i]['Title.' + str(l)] = num
    return articles
        
def addArticleTextModelProcessedData(articles):
    model = Doc2Vec.load('D:\\wamp\\tmp\\articleTextModel.mod')
    for i, article in enumerate(articles):
        tokens = gensim.utils.simple_preprocess(article['Text'])
        vector = model.infer_vector(tokens)
        for l, num in enumerate(vector):
            articles[i]['Text.' + str(l)] = num
    return articles


def addTagData(articles):
    uniqueTags = getUniqueTags(articles)
    uniqueTags = ['Tags.' + str(x) for x in uniqueTags]
    
    for i, article in enumerate(articles):
        tags = article['Tags']
        for tag in tags:
            colName = 'Tags.' + str(tag['_id'])
            articles[i][colName] = 1.0
    return articles
        

def getUniqueTags(data):
    seen = set()
    unique = []
    for item in data:
        d = [o['_id'] for o in item.get('Tags')]
        for index in d:
            if index not in seen:
                seen.add(index)
                unique.append(index)
    unique.sort()    
    return unique
    

def getMongoClient():
    return pymongo.MongoClient("mongodb://admin:Pa$$w0rd@localhost:27017/")
    
def mongoGetArticles():
    mongoClient = getMongoClient()
    dbArticles = mongoClient["articleDataFinal"]
    articleCollection = dbArticles["articles"]
    return list(articleCollection.find({}))

def mongoGetArticleWordOccurence():
    mongoClient = getMongoClient()
    dbWordData = mongoClient["wordData"]
    articleWordOccurenceCollection = dbWordData["articleWordOccurence100Words"]
    return list(articleWordOccurenceCollection.find({}))


def GetNonVideoFocusedArticles(articles):
    return [article for article in articles if article['VideoFocusedArticle'] == False]

def addPopularColumn(articles):
    for i, article in enumerate(articles):
        if (article['Views'] > 3850): #3850
            articles[i]['Popular'] = 1
        else:
            articles[i]['Popular'] = 0
    return articles

articles = mongoGetArticles()
articleWordOccurenceData = mongoGetArticleWordOccurence()


#articles = GetNonVideoFocusedArticles(articles)

articles = addPopularColumn(articles)

articles = addWordColumnsAndData(articles, articleWordOccurenceData)

articles = addTitleModelProcessedData(articles)

articles = addArticleTextModelProcessedData(articles)

articles = addTagData(articles)


#aaa = [article for article in articles if article['ActivePeriod'] < 2772887]
#articles = aaa


df = pd.DataFrame(articles)

#df = df[31:].fillna(0) #tags NAN values to 0 # 116 for no titles
df = df.fillna(0)


df['Author'] = np.where((df.Author == 0), 'უცნობი', df.Author)
df['VideoFocusedArticle'] = np.where((df.VideoFocusedArticle == True), 1, 0)
df['BannerInsideArticle'] = np.where((df.BannerInsideArticle == True), 1, 0)
df['PublishedOnSocialMedia'] = np.where((df.PublishedOnSocialMedia == True), 1, 0)
df['BoostedOnSocialMedia'] = np.where((df.BoostedOnSocialMedia == True), 1, 0)

df = df.drop('Tags',1)
df = df.drop('Text',1)
df = df.drop('Title',1)
df = df.drop('Author',1)


#df = df.drop('Tags.161',1)
#df = df.drop('Tags.138',1)
#df = df.drop('Tags.140',1)
#df = df.drop('Tags.164',1)
#df = df.drop('Tags.162',1)


#ct = ColumnTransformer([("Author", OneHotEncoder(), [8])], remainder = 'passthrough')
#npArray  = ct.fit_transform(df)

#df['Popular'] = np.random.randint(0, 2, df.shape[0])
#df = df.loc[df['ActivePeriod'] < 2772887]

npArray = np.array(df)
#npArray = sc.fit_transform(npArray)

X = np.delete(npArray, [0,2,12], 1) 
Y = npArray[:,12]


scX = MinMaxScaler()
scY = MinMaxScaler()

X = scX.fit_transform(X)
Y = scY.fit_transform(Y.reshape(-1,1))

X_train, X_test, Y_train, Y_test = train_test_split(X, Y, test_size = 0.3, shuffle = True) #TRIBUTE: Hitchhiker's Guide to the Galaxy 

classifier = build_classifier(len(X_train[0]))
classifier.fit(X_train, Y_train, batch_size = 10, epochs = 250)

#classifier = keras.models.load_model('D:\\wamp\\tmp\\MLClassifierModel_24_02_2021.mod')

#classifier.save('D:\\wamp\\tmp\\MLClassifierModel_24_02_2021.mod')

Y_pred = classifier.predict(X_test)

Y_pred = scY.inverse_transform(Y_pred)
Y_test = scY.inverse_transform(Y_test)

loss, acc = classifier.evaluate(X_test, Y_test, verbose=0)



plt.figure(figsize=(160, 160), dpi=80)

plt.plot(Y_test)
plt.plot(Y_pred)

# plt.scatter(Y_test, Y_pred)

# plt.gca().set_aspect('auto')
# res = np.arange(1,5826)
# plt.plot((np.array(Y_test)).flatten())

# plt.style.use('seaborn-whitegrid')

